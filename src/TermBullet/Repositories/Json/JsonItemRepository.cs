using TermBullet.Services.Clock;
using TermBullet.Services.Maintenance;
using System.Text.Json;
using System.Text.Json.Serialization;
using TermBullet.Repositories.Interfaces;
using TermBullet.Domain.Items;
using TermBullet.Domain.Refs;

namespace TermBullet.Repositories.Json;

public sealed class JsonItemRepository(
    IClock clock,
    MonthlyJsonPathResolver pathResolver,
    JsonFileStore fileStore,
    JsonIndexService? indexService = null) : IItemRepository, IItemArchiveReader, IMonthRolloverService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
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

        var (year, month) = GetPeriodFromPublicRef(item.PublicRef);
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
            data: BuildUpdateHistoryData(previous, current));

        await WriteMonthlyDocumentAsync(year, month, document, cancellationToken);
        await RebuildIndexIfConfiguredAsync(cancellationToken);
    }

    public async Task DeleteByPublicRefAsync(string publicRef, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(publicRef);
        var parsedPublicRef = PublicRef.Parse(publicRef);

        var (year, month) = GetPeriodFromPublicRef(parsedPublicRef);
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
        var parsedPublicRef = PublicRef.Parse(publicRef);
        var (year, month) = GetPeriodFromPublicRef(parsedPublicRef);
        var document = await ReadMonthlyDocumentByPeriodAsync(year, month, cancellationToken);
        var storageItem = document.Items.FirstOrDefault(item => item.PublicRef == parsedPublicRef.Value);
        return storageItem is null ? null : ToDomainItem(storageItem);
    }

    public async Task<IReadOnlyCollection<Item>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        var dataRoot = Path.Combine(pathResolver.ProjectRootPath, "data");
        if (!Directory.Exists(dataRoot))
        {
            return [];
        }

        var items = new List<Item>();
        foreach (var monthlyPath in Directory.EnumerateFiles(dataRoot, "data_??_????.json", SearchOption.AllDirectories))
        {
            var fileName = Path.GetFileName(monthlyPath);
            if (!TryParseMonthlyFileName(fileName, out var month, out var year))
            {
                continue;
            }

            var document = await ReadMonthlyDocumentByPeriodAsync(year, month, cancellationToken);
            items.AddRange(document.Items.Select(ToDomainItem));
        }

        return items
            .OrderBy(item => item.CreatedAt)
            .ThenBy(item => item.PublicRef.Value, StringComparer.Ordinal)
            .ToArray();
    }

    public async Task RunAutomaticMonthRolloverAsync(CancellationToken cancellationToken = default)
    {
        var (currentYear, currentMonth) = GetCurrentPeriod();
        var currentDocument = await ReadMonthlyDocumentByPeriodAsync(currentYear, currentMonth, cancellationToken);
        await WriteMonthlyDocumentAsync(currentYear, currentMonth, currentDocument, cancellationToken);
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
        return document;
    }

    private static bool TryParseMonthlyFileName(string fileName, out int month, out int year)
    {
        month = 0;
        year = 0;

        if (!fileName.StartsWith("data_", StringComparison.OrdinalIgnoreCase)
            || !fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var parts = Path.GetFileNameWithoutExtension(fileName).Split('_');
        return parts.Length == 3
            && int.TryParse(parts[1], out month)
            && int.TryParse(parts[2], out year)
            && month is >= 1 and <= 12;
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

    private (int Year, int Month) GetPeriodFromPublicRef(PublicRef publicRef)
    {
        var (currentYear, _) = GetCurrentPeriod();
        return GetPeriodFromPublicRef(publicRef, currentYear);
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
                _ => "edited"
            };
        }

        if (!string.Equals(previous.Collection, current.Collection, StringComparison.Ordinal))
        {
            return "migrate";
        }

        return "edited";
    }

    private static object BuildUpdateHistoryData(StorageItem previous, StorageItem current)
    {
        if (!string.Equals(previous.Collection, current.Collection, StringComparison.Ordinal))
        {
            return new
            {
                public_ref = current.PublicRef,
                from_collection = previous.Collection,
                to_collection = current.Collection
            };
        }

        return new
        {
            status = current.Status,
            collection = current.Collection,
            priority = current.Priority,
            version = current.Version
        };
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
            ScheduledAt = item.ScheduledAt,
            Version = item.Version,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt,
            CompletedAt = item.CompletedAt,
            CancelledAt = item.CancelledAt
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
            item.ScheduledAt,
            item.CompletedAt,
            item.CancelledAt);
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
            ItemStatus.Done => "done",
            ItemStatus.Cancelled => "cancelled",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unsupported item status.")
        };

    private static ItemStatus ParseStatus(string value) =>
        value switch
        {
            "open" => ItemStatus.Open,
            "done" => ItemStatus.Done,
            "cancelled" => ItemStatus.Cancelled,
            _ => throw new InvalidDataException($"Unsupported item status value: {value}.")
        };

    private static string ToCollectionKey(ItemCollection collection) =>
        collection switch
        {
            ItemCollection.Today => "today",
            ItemCollection.Week => "week",
            ItemCollection.Month => "month",
            ItemCollection.Backlog => "backlog",
            _ => throw new ArgumentOutOfRangeException(nameof(collection), collection, "Unsupported item collection.")
        };

    private static ItemCollection ParseCollection(string value) =>
        value switch
        {
            "today" => ItemCollection.Today,
            "week" => ItemCollection.Week,
            "month" => ItemCollection.Month,
            "backlog" => ItemCollection.Backlog,
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

}
