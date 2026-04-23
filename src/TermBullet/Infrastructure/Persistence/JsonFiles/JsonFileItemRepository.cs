using System.Text.Json;
using System.Text.Json.Serialization;
using TermBullet.Application.Ports;
using TermBullet.Core.Items;
using TermBullet.Core.Refs;

namespace TermBullet.Infrastructure.Persistence.JsonFiles;

public sealed class JsonFileItemRepository(
    IClock clock,
    MonthlyJsonFilePathResolver pathResolver,
    SafeJsonFileStore fileStore,
    LocalJsonIndexService? indexService = null) : IItemRepository, IMonthRolloverService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<int> GetCurrentPublicRefSequenceAsync(
        ItemType type,
        int month,
        int year,
        CancellationToken cancellationToken = default)
    {
        var monthlyPath = pathResolver.ResolveMonthlyFilePath(year, month);
        if (!File.Exists(monthlyPath))
        {
            return 0;
        }

        var document = await ReadMonthlyDocumentByPeriodAsync(year, month, cancellationToken);
        return document.PublicRefSequences.TryGetValue(ToTypeKey(type), out var value)
            ? value
            : 0;
    }

    public async Task<bool> PublicRefExistsAsync(
        string publicRef,
        CancellationToken cancellationToken = default)
    {
        var document = await ReadCurrentMonthlyDocumentAsync(cancellationToken);
        return document.Items.Any(item => string.Equals(item.PublicRef, publicRef, StringComparison.Ordinal));
    }

    public async Task AddAsync(Item item, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);

        var (year, month) = GetCurrentPeriod();
        var document = await ReadMonthlyDocumentByPeriodAsync(year, month, cancellationToken);
        if (document.Items.Any(existing => string.Equals(existing.PublicRef, item.PublicRef.Value, StringComparison.Ordinal)))
        {
            throw new InvalidOperationException($"Public ref already exists in current month: {item.PublicRef.Value}.");
        }

        var storageItem = ToStorageItem(item);
        document.Items.Add(storageItem);
        UpdateSequence(document, item.Type, item.PublicRef.Sequence);
        AppendHistory(
            document,
            itemId: item.Id,
            publicRef: item.PublicRef.Value,
            eventType: "created",
            data: new
            {
                content = item.Content,
                status = ToStatusKey(item.Status),
                collection = ToCollectionKey(item.Collection)
            });

        await WriteMonthlyDocumentAsync(year, month, document, cancellationToken);
        await RebuildIndexIfConfiguredAsync(cancellationToken);
    }

    public async Task UpdateAsync(Item item, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);

        var (year, month) = GetCurrentPeriod();
        var document = await ReadMonthlyDocumentByPeriodAsync(year, month, cancellationToken);
        var index = document.Items.FindIndex(existing => existing.Id == item.Id);
        if (index < 0)
        {
            throw new KeyNotFoundException($"Item not found for update: {item.PublicRef.Value}.");
        }

        var previous = document.Items[index];
        var current = ToStorageItem(item);
        document.Items[index] = current;
        UpdateSequence(document, item.Type, item.PublicRef.Sequence);
        AppendHistory(
            document,
            itemId: item.Id,
            publicRef: item.PublicRef.Value,
            eventType: GetUpdateEventType(previous, current),
            data: new
            {
                status = current.Status,
                collection = current.Collection,
                priority = current.Priority,
                version = current.Version
            });

        await WriteMonthlyDocumentAsync(year, month, document, cancellationToken);
        await RebuildIndexIfConfiguredAsync(cancellationToken);
    }

    public async Task DeleteByPublicRefAsync(string publicRef, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(publicRef);
        var parsedPublicRef = PublicRef.Parse(publicRef);

        var (year, month) = GetCurrentPeriod();
        var document = await ReadMonthlyDocumentByPeriodAsync(year, month, cancellationToken);
        var index = document.Items.FindIndex(existing => string.Equals(
            existing.PublicRef,
            parsedPublicRef.Value,
            StringComparison.Ordinal));
        if (index < 0)
        {
            throw new KeyNotFoundException($"Item not found for delete: {parsedPublicRef.Value}.");
        }

        var deleted = document.Items[index];
        document.Items.RemoveAt(index);
        AppendHistory(
            document,
            itemId: deleted.Id,
            publicRef: deleted.PublicRef,
            eventType: "deleted",
            data: new
            {
                snapshot = deleted
            });

        await WriteMonthlyDocumentAsync(year, month, document, cancellationToken);
        await RebuildIndexIfConfiguredAsync(cancellationToken);
    }

    public async Task ClearHistoryAsync(CancellationToken cancellationToken = default)
    {
        var (year, month) = GetCurrentPeriod();
        var document = await ReadMonthlyDocumentByPeriodAsync(year, month, cancellationToken);
        document.History.Clear();
        await WriteMonthlyDocumentAsync(year, month, document, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Item>> ListAsync(
        ItemCollection? collection = null,
        ItemStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var document = await ReadCurrentMonthlyDocumentAsync(cancellationToken);
        var items = document.Items.Select(ToDomainItem);

        if (collection is not null)
        {
            items = items.Where(item => item.Collection == collection.Value);
        }

        if (status is not null)
        {
            items = items.Where(item => item.Status == status.Value);
        }

        return items.ToArray();
    }

    public async Task<Item?> FindByPublicRefAsync(
        string publicRef,
        CancellationToken cancellationToken = default)
    {
        var document = await ReadCurrentMonthlyDocumentAsync(cancellationToken);
        var storageItem = document.Items.FirstOrDefault(item => item.PublicRef == publicRef);
        return storageItem is null ? null : ToDomainItem(storageItem);
    }

    public async Task RunAutomaticMonthRolloverAsync(CancellationToken cancellationToken = default)
    {
        var (currentYear, currentMonth) = GetCurrentPeriod();
        var currentPeriod = $"{currentYear:0000}-{currentMonth:00}";
        var previousDate = new DateTimeOffset(currentYear, currentMonth, 1, 0, 0, 0, TimeSpan.Zero).AddMonths(-1);
        var previousYear = previousDate.Year;
        var previousMonth = previousDate.Month;
        var previousPeriod = $"{previousYear:0000}-{previousMonth:00}";

        var sourceDocument = await ReadMonthlyDocumentByPeriodAsync(previousYear, previousMonth, cancellationToken);
        if (sourceDocument.Items.Count == 0)
        {
            return;
        }

        var destinationDocument = await ReadMonthlyDocumentByPeriodAsync(currentYear, currentMonth, cancellationToken);
        var candidates = sourceDocument.Items
            .Where(IsAutomaticRolloverCandidate)
            .ToArray();

        if (candidates.Length == 0)
        {
            return;
        }

        var now = clock.UtcNow;
        foreach (var sourceItem in candidates)
        {
            var destinationIndex = destinationDocument.Items.FindIndex(existing =>
                string.Equals(existing.PublicRef, sourceItem.PublicRef, StringComparison.Ordinal));

            if (destinationIndex >= 0)
            {
                var existing = destinationDocument.Items[destinationIndex];
                if (existing.Id != sourceItem.Id)
                {
                    throw new InvalidOperationException(
                        $"Automatic rollover detected conflicting public ref in destination month: {sourceItem.PublicRef}.");
                }
            }
            else
            {
                var migratedItem = CreateRolloverDestinationItem(sourceItem, previousPeriod, currentPeriod, now);
                destinationDocument.Items.Add(migratedItem);
                UpdateSequence(destinationDocument, ParseType(migratedItem.Type), PublicRef.Parse(migratedItem.PublicRef).Sequence);
                AppendHistory(
                    destinationDocument,
                    migratedItem.Id,
                    migratedItem.PublicRef,
                    "migrated",
                    new
                    {
                        from_period = previousPeriod,
                        to_period = currentPeriod,
                        reason = "automatic_month_rollover"
                    });
            }

            sourceDocument.Items.RemoveAll(existing => existing.Id == sourceItem.Id);
            AppendHistory(
                sourceDocument,
                sourceItem.Id,
                sourceItem.PublicRef,
                "migrated",
                new
                {
                    from_period = previousPeriod,
                    to_period = currentPeriod,
                    reason = "automatic_month_rollover"
                });
        }

        await WriteMonthlyDocumentAsync(previousYear, previousMonth, sourceDocument, cancellationToken);
        await WriteMonthlyDocumentAsync(currentYear, currentMonth, destinationDocument, cancellationToken);
        await RebuildIndexIfConfiguredAsync(cancellationToken);
    }

    private async Task<MonthlyDataDocument> ReadCurrentMonthlyDocumentAsync(CancellationToken cancellationToken)
    {
        var (year, month) = GetCurrentPeriod();
        return await ReadMonthlyDocumentByPeriodAsync(year, month, cancellationToken);
    }

    private async Task<MonthlyDataDocument> ReadMonthlyDocumentByPeriodAsync(
        int year,
        int month,
        CancellationToken cancellationToken)
    {
        var monthlyPath = pathResolver.ResolveMonthlyFilePath(year, month);
        if (!File.Exists(monthlyPath))
        {
            return MonthlyDataDocument.CreateEmpty(year, month);
        }

        var backupPath = pathResolver.ResolveBackupFilePath(year, month);
        var json = await fileStore.ReadOrRecoverAsync(monthlyPath, backupPath, cancellationToken);
        var document = JsonSerializer.Deserialize<MonthlyDataDocument>(json, JsonOptions)
            ?? throw new InvalidDataException("Monthly data file could not be deserialized.");

        document.Items ??= [];
        document.PublicRefSequences ??= new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        document.Period ??= $"{year:0000}-{month:00}";
        document.FileName ??= $"data_{month:00}_{year:0000}.json";
        document.History ??= [];
        document.Settings ??= new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);

        return document;
    }

    private async Task WriteMonthlyDocumentAsync(
        int year,
        int month,
        MonthlyDataDocument document,
        CancellationToken cancellationToken)
    {
        var monthlyPath = pathResolver.ResolveMonthlyFilePath(year, month);
        var backupPath = pathResolver.ResolveBackupFilePath(year, month);

        document.Period ??= $"{year:0000}-{month:00}";
        document.FileName ??= Path.GetFileName(monthlyPath);

        var json = JsonSerializer.Serialize(document, JsonOptions);
        await fileStore.WriteAsync(monthlyPath, backupPath, json, cancellationToken);
    }

    private static (int Year, int Month) GetPeriodFromPublicRef(PublicRef publicRef, int fallbackCenturyYear)
    {
        var fallbackCentury = fallbackCenturyYear / 100;
        var year = fallbackCentury * 100 + publicRef.YearTwoDigits;
        return (year, publicRef.Month);
    }

    private (int Year, int Month) GetCurrentPeriod()
    {
        var now = clock.UtcNow;
        return (now.Year, now.Month);
    }

    private Task RebuildIndexIfConfiguredAsync(CancellationToken cancellationToken)
    {
        return indexService is null
            ? Task.CompletedTask
            : indexService.RebuildAsync(cancellationToken);
    }

    private static void UpdateSequence(MonthlyDataDocument document, ItemType type, int sequence)
    {
        var key = ToTypeKey(type);
        if (!document.PublicRefSequences.TryGetValue(key, out var current) || sequence > current)
        {
            document.PublicRefSequences[key] = sequence;
        }
    }

    private static void AppendHistory(
        MonthlyDataDocument document,
        Guid itemId,
        string publicRef,
        string eventType,
        object? data = null)
    {
        document.History.Add(new HistoryEntry
        {
            Id = Guid.NewGuid(),
            ItemId = itemId,
            PublicRef = publicRef,
            EventType = eventType,
            OccurredAt = DateTimeOffset.UtcNow,
            Data = ToJsonElement(data)
        });
    }

    private static string GetUpdateEventType(StorageItem previous, StorageItem current)
    {
        if (!string.Equals(previous.Status, current.Status, StringComparison.Ordinal))
        {
            return current.Status switch
            {
                "done" => "done",
                "cancelled" => "cancelled",
                "migrated" => "migrated",
                _ => "edited"
            };
        }

        return "edited";
    }

    private static JsonElement ToJsonElement(object? value)
    {
        var serialized = JsonSerializer.Serialize(value ?? new { });
        using var doc = JsonDocument.Parse(serialized);
        return doc.RootElement.Clone();
    }

    private static StorageItem ToStorageItem(Item item)
    {
        return new StorageItem
        {
            Id = item.Id,
            PublicRef = item.PublicRef.Value,
            Type = ToTypeKey(item.Type),
            Content = item.Content,
            Description = item.Description,
            Status = ToStatusKey(item.Status),
            Collection = ToCollectionKey(item.Collection),
            Priority = ToPriorityKey(item.Priority),
            Tags = [.. item.Tags],
            DueAt = item.DueAt,
            ScheduledAt = item.ScheduledAt,
            EstimateMinutes = item.EstimateMinutes,
            Version = item.Version,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt,
            CompletedAt = item.CompletedAt,
            CancelledAt = item.CancelledAt,
            MigratedAt = item.MigratedAt,
            Migration = item.Migration is null
                ? null
                : new StorageMigration
                {
                    FromPeriod = item.Migration.FromPeriod,
                    ToPeriod = item.Migration.ToPeriod,
                    MigratedAt = item.Migration.MigratedAt,
                    Reason = item.Migration.Reason
                }
        };
    }

    private Item ToDomainItem(StorageItem item)
    {
        var type = ParseType(item.Type);
        var publicRef = PublicRef.Parse(item.PublicRef);
        var (year, month) = GetCurrentPeriod();
        var (refYear, refMonth) = GetPeriodFromPublicRef(publicRef, year);

        // Keep ref period coherent with its own MMYY segment.
        if (refMonth != publicRef.Month || refYear % 100 != publicRef.YearTwoDigits)
        {
            throw new InvalidDataException($"Invalid public ref period: {publicRef.Value}.");
        }

        return Item.Restore(
            item.Id,
            publicRef,
            type,
            item.Content,
            item.Description,
            ParseStatus(item.Status),
            ParseCollection(item.Collection),
            ParsePriority(item.Priority),
            item.Tags,
            item.Version,
            item.CreatedAt,
            item.UpdatedAt,
            item.DueAt,
            item.ScheduledAt,
            item.EstimateMinutes,
            item.CompletedAt,
            item.CancelledAt,
            item.MigratedAt,
            item.Migration is null
                ? null
                : new MigrationInfo(
                    item.Migration.FromPeriod,
                    item.Migration.ToPeriod,
                    item.Migration.MigratedAt,
                    item.Migration.Reason));
    }

    private static bool IsAutomaticRolloverCandidate(StorageItem item) =>
        string.Equals(item.Type, "task", StringComparison.Ordinal)
        && item.Status is "open" or "in_progress";

    private static StorageItem CreateRolloverDestinationItem(
        StorageItem sourceItem,
        string previousPeriod,
        string currentPeriod,
        DateTimeOffset now)
    {
        return new StorageItem
        {
            Id = sourceItem.Id,
            PublicRef = sourceItem.PublicRef,
            Type = sourceItem.Type,
            Content = sourceItem.Content,
            Description = sourceItem.Description,
            Status = sourceItem.Status,
            Collection = sourceItem.Collection,
            Priority = sourceItem.Priority,
            Tags = [.. sourceItem.Tags],
            DueAt = sourceItem.DueAt,
            ScheduledAt = sourceItem.ScheduledAt,
            EstimateMinutes = sourceItem.EstimateMinutes,
            Version = sourceItem.Version + 1,
            CreatedAt = sourceItem.CreatedAt,
            UpdatedAt = now,
            CompletedAt = sourceItem.CompletedAt,
            CancelledAt = sourceItem.CancelledAt,
            MigratedAt = now,
            Migration = new StorageMigration
            {
                FromPeriod = previousPeriod,
                ToPeriod = currentPeriod,
                MigratedAt = now,
                Reason = "automatic_month_rollover"
            }
        };
    }

    private static string ToTypeKey(ItemType type) =>
        type switch
        {
            ItemType.Task => "task",
            ItemType.Note => "note",
            ItemType.Event => "event",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported item type.")
        };

    private static ItemType ParseType(string value) =>
        value switch
        {
            "task" => ItemType.Task,
            "note" => ItemType.Note,
            "event" => ItemType.Event,
            _ => throw new InvalidDataException($"Unsupported item type value: {value}.")
        };

    private static string ToStatusKey(ItemStatus status) =>
        status switch
        {
            ItemStatus.Open => "open",
            ItemStatus.InProgress => "in_progress",
            ItemStatus.Done => "done",
            ItemStatus.Cancelled => "cancelled",
            ItemStatus.Migrated => "migrated",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unsupported item status.")
        };

    private static ItemStatus ParseStatus(string value) =>
        value switch
        {
            "open" => ItemStatus.Open,
            "in_progress" => ItemStatus.InProgress,
            "done" => ItemStatus.Done,
            "cancelled" => ItemStatus.Cancelled,
            "migrated" => ItemStatus.Migrated,
            _ => throw new InvalidDataException($"Unsupported item status value: {value}.")
        };

    private static string ToCollectionKey(ItemCollection collection) =>
        collection switch
        {
            ItemCollection.Today => "today",
            ItemCollection.Week => "week",
            ItemCollection.Backlog => "backlog",
            ItemCollection.Monthly => "monthly",
            ItemCollection.Archived => "archived",
            _ => throw new ArgumentOutOfRangeException(nameof(collection), collection, "Unsupported item collection.")
        };

    private static ItemCollection ParseCollection(string value) =>
        value switch
        {
            "today" => ItemCollection.Today,
            "week" => ItemCollection.Week,
            "backlog" => ItemCollection.Backlog,
            "monthly" => ItemCollection.Monthly,
            "archived" => ItemCollection.Archived,
            _ => throw new InvalidDataException($"Unsupported item collection value: {value}.")
        };

    private static string ToPriorityKey(Priority priority) =>
        priority switch
        {
            Priority.None => "none",
            Priority.Low => "low",
            Priority.Medium => "medium",
            Priority.High => "high",
            _ => throw new ArgumentOutOfRangeException(nameof(priority), priority, "Unsupported priority.")
        };

    private static Priority ParsePriority(string value) =>
        value switch
        {
            "none" => Priority.None,
            "low" => Priority.Low,
            "medium" => Priority.Medium,
            "high" => Priority.High,
            _ => throw new InvalidDataException($"Unsupported priority value: {value}.")
        };

    private sealed class MonthlyDataDocument
    {
        [JsonPropertyName("period")]
        public string? Period { get; set; }

        [JsonPropertyName("file_name")]
        public string? FileName { get; set; }

        [JsonPropertyName("public_ref_sequences")]
        public Dictionary<string, int> PublicRefSequences { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        [JsonPropertyName("items")]
        public List<StorageItem> Items { get; set; } = [];

        [JsonPropertyName("history")]
        public List<HistoryEntry> History { get; set; } = [];

        [JsonPropertyName("settings")]
        public Dictionary<string, JsonElement> Settings { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        public static MonthlyDataDocument CreateEmpty(int year, int month)
        {
            return new MonthlyDataDocument
            {
                Period = $"{year:0000}-{month:00}",
                FileName = $"data_{month:00}_{year:0000}.json",
                PublicRefSequences = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    ["task"] = 0,
                    ["note"] = 0,
                    ["event"] = 0
                }
            };
        }
    }

    private sealed class StorageItem
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("public_ref")]
        public string PublicRef { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "open";

        [JsonPropertyName("collection")]
        public string Collection { get; set; } = "today";

        [JsonPropertyName("priority")]
        public string Priority { get; set; } = "none";

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = [];

        [JsonPropertyName("due_at")]
        public DateTimeOffset? DueAt { get; set; }

        [JsonPropertyName("scheduled_at")]
        public DateTimeOffset? ScheduledAt { get; set; }

        [JsonPropertyName("estimate_minutes")]
        public int? EstimateMinutes { get; set; }

        [JsonPropertyName("version")]
        public int Version { get; set; } = 1;

        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonPropertyName("completed_at")]
        public DateTimeOffset? CompletedAt { get; set; }

        [JsonPropertyName("cancelled_at")]
        public DateTimeOffset? CancelledAt { get; set; }

        [JsonPropertyName("migrated_at")]
        public DateTimeOffset? MigratedAt { get; set; }

        [JsonPropertyName("migration")]
        public StorageMigration? Migration { get; set; }
    }

    private sealed class StorageMigration
    {
        [JsonPropertyName("from_period")]
        public string FromPeriod { get; set; } = string.Empty;

        [JsonPropertyName("to_period")]
        public string ToPeriod { get; set; } = string.Empty;

        [JsonPropertyName("migrated_at")]
        public DateTimeOffset MigratedAt { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; } = string.Empty;
    }

    private sealed class HistoryEntry
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("item_id")]
        public Guid ItemId { get; set; }

        [JsonPropertyName("public_ref")]
        public string PublicRef { get; set; } = string.Empty;

        [JsonPropertyName("event_type")]
        public string EventType { get; set; } = string.Empty;

        [JsonPropertyName("occurred_at")]
        public DateTimeOffset OccurredAt { get; set; }

        [JsonPropertyName("data")]
        public JsonElement Data { get; set; }
    }
}
