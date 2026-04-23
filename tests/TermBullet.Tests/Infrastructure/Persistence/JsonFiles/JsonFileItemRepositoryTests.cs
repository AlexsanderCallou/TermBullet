using System.Text.Json;
using TermBullet.Application.Ports;
using TermBullet.Core.Items;
using TermBullet.Core.Refs;
using TermBullet.Infrastructure.Persistence.JsonFiles;

namespace TermBullet.Tests.Infrastructure.Persistence.JsonFiles;

public sealed class JsonFileItemRepositoryTests
{
    [Fact]
    public async Task GetCurrentPublicRefSequenceAsync_returns_zero_when_monthly_file_does_not_exist()
    {
        var repository = CreateRepository();

        var sequence = await repository.GetCurrentPublicRefSequenceAsync(ItemType.Task, 4, 2026);

        Assert.Equal(0, sequence);
    }

    [Fact]
    public async Task AddAsync_creates_monthly_file_and_allows_find_by_public_ref()
    {
        var context = CreateContext();
        var repository = CreateRepository(context);
        var item = CreateItem(
            id: Guid.Parse("0f3a9d94-4df0-47f7-95c1-0f967c22f4db"),
            publicRef: "t-0426-1",
            collection: ItemCollection.Today);

        await repository.AddAsync(item);
        var found = await repository.FindByPublicRefAsync("t-0426-1");

        Assert.NotNull(found);
        Assert.Equal(item.Id, found.Id);
        Assert.Equal("t-0426-1", found.PublicRef.Value);
        Assert.Equal(ItemCollection.Today, found.Collection);
        Assert.True(File.Exists(context.MonthlyFilePath));
        Assert.True(File.Exists(context.IndexFilePath));
    }

    [Fact]
    public async Task AddAsync_updates_public_ref_sequence_for_type_and_month()
    {
        var repository = CreateRepository();
        var item = CreateItem(
            id: Guid.Parse("0f3a9d94-4df0-47f7-95c1-0f967c22f4db"),
            publicRef: "t-0426-3",
            collection: ItemCollection.Today);

        await repository.AddAsync(item);

        var sequence = await repository.GetCurrentPublicRefSequenceAsync(ItemType.Task, 4, 2026);

        Assert.Equal(3, sequence);
    }

    [Fact]
    public async Task UpdateAsync_persists_item_changes()
    {
        var context = CreateContext();
        var repository = CreateRepository(context);
        var item = CreateItem(
            id: Guid.Parse("0f3a9d94-4df0-47f7-95c1-0f967c22f4db"),
            publicRef: "t-0426-1",
            collection: ItemCollection.Today);
        await repository.AddAsync(item);

        item.Edit("Refactor auth flow", ChangedAt, description: "keep tests green");
        item.MoveTo(ItemCollection.Backlog, ChangedAt);
        item.SetPriority(Priority.High, ChangedAt);
        await repository.UpdateAsync(item);

        var updated = await repository.FindByPublicRefAsync("t-0426-1");
        Assert.NotNull(updated);
        Assert.Equal("Refactor auth flow", updated.Content);
        Assert.Equal("keep tests green", updated.Description);
        Assert.Equal(ItemCollection.Backlog, updated.Collection);
        Assert.Equal(Priority.High, updated.Priority);
        Assert.Equal(item.Version, updated.Version);

        var indexJson = await File.ReadAllTextAsync(context.IndexFilePath);
        using var indexDoc = JsonDocument.Parse(indexJson);
        var indexItem = Assert.Single(indexDoc.RootElement.GetProperty("items").EnumerateArray());
        Assert.Equal("backlog", indexItem.GetProperty("collection").GetString());
        Assert.Equal("high", indexItem.GetProperty("priority").GetString());
    }

    [Fact]
    public async Task ListAsync_filters_by_collection_and_status()
    {
        var repository = CreateRepository();
        var openToday = CreateItem(
            id: Guid.Parse("0f3a9d94-4df0-47f7-95c1-0f967c22f4db"),
            publicRef: "t-0426-1",
            collection: ItemCollection.Today);
        var doneBacklog = CreateItem(
            id: Guid.Parse("c4dbec0e-c42d-4f26-8659-05bfdb4db056"),
            publicRef: "t-0426-2",
            collection: ItemCollection.Backlog);
        doneBacklog.MarkDone(ChangedAt);
        await repository.AddAsync(openToday);
        await repository.AddAsync(doneBacklog);

        var byCollection = await repository.ListAsync(collection: ItemCollection.Backlog);
        var byStatus = await repository.ListAsync(status: ItemStatus.Done);

        var collectionItem = Assert.Single(byCollection);
        var statusItem = Assert.Single(byStatus);
        Assert.Equal("t-0426-2", collectionItem.PublicRef.Value);
        Assert.Equal("t-0426-2", statusItem.PublicRef.Value);
    }

    [Fact]
    public async Task AddAsync_rejects_duplicate_public_ref_in_same_month()
    {
        var repository = CreateRepository();
        await repository.AddAsync(CreateItem(
            id: Guid.Parse("0f3a9d94-4df0-47f7-95c1-0f967c22f4db"),
            publicRef: "t-0426-1",
            collection: ItemCollection.Today));

        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.AddAsync(CreateItem(
            id: Guid.Parse("c4dbec0e-c42d-4f26-8659-05bfdb4db056"),
            publicRef: "t-0426-1",
            collection: ItemCollection.Backlog)));
    }

    [Fact]
    public async Task AddAsync_appends_created_event_to_history()
    {
        var context = CreateContext();
        var repository = CreateRepository(context);
        var item = CreateItem(
            id: Guid.Parse("0f3a9d94-4df0-47f7-95c1-0f967c22f4db"),
            publicRef: "t-0426-1",
            collection: ItemCollection.Today);

        await repository.AddAsync(item);

        var json = await File.ReadAllTextAsync(context.MonthlyFilePath);
        using var doc = JsonDocument.Parse(json);
        var history = doc.RootElement.GetProperty("history");
        var entry = Assert.Single(history.EnumerateArray());
        Assert.Equal("created", entry.GetProperty("event_type").GetString());
        Assert.Equal("t-0426-1", entry.GetProperty("public_ref").GetString());
    }

    [Fact]
    public async Task AddAsync_writes_optional_schema_fields_as_null_when_absent()
    {
        var context = CreateContext();
        var repository = CreateRepository(context);
        var item = CreateItem(
            id: Guid.Parse("0f3a9d94-4df0-47f7-95c1-0f967c22f4db"),
            publicRef: "t-0426-1",
            collection: ItemCollection.Today);

        await repository.AddAsync(item);

        var json = await File.ReadAllTextAsync(context.MonthlyFilePath);
        using var doc = JsonDocument.Parse(json);
        var stored = Assert.Single(doc.RootElement.GetProperty("items").EnumerateArray());
        Assert.True(stored.TryGetProperty("description", out var description));
        Assert.Equal(JsonValueKind.Null, description.ValueKind);
        Assert.True(stored.TryGetProperty("due_at", out var dueAt));
        Assert.Equal(JsonValueKind.Null, dueAt.ValueKind);
        Assert.True(stored.TryGetProperty("scheduled_at", out var scheduledAt));
        Assert.Equal(JsonValueKind.Null, scheduledAt.ValueKind);
        Assert.True(stored.TryGetProperty("estimate_minutes", out var estimateMinutes));
        Assert.Equal(JsonValueKind.Null, estimateMinutes.ValueKind);
        Assert.True(stored.TryGetProperty("completed_at", out var completedAt));
        Assert.Equal(JsonValueKind.Null, completedAt.ValueKind);
        Assert.True(stored.TryGetProperty("cancelled_at", out var cancelledAt));
        Assert.Equal(JsonValueKind.Null, cancelledAt.ValueKind);
        Assert.True(stored.TryGetProperty("migrated_at", out var migratedAt));
        Assert.Equal(JsonValueKind.Null, migratedAt.ValueKind);
        Assert.True(stored.TryGetProperty("migration", out var migration));
        Assert.Equal(JsonValueKind.Null, migration.ValueKind);
    }

    [Fact]
    public async Task UpdateAsync_appends_status_event_to_history()
    {
        var context = CreateContext();
        var repository = CreateRepository(context);
        var item = CreateItem(
            id: Guid.Parse("0f3a9d94-4df0-47f7-95c1-0f967c22f4db"),
            publicRef: "t-0426-1",
            collection: ItemCollection.Today);
        await repository.AddAsync(item);
        item.MarkDone(ChangedAt);

        await repository.UpdateAsync(item);

        var json = await File.ReadAllTextAsync(context.MonthlyFilePath);
        using var doc = JsonDocument.Parse(json);
        var history = doc.RootElement.GetProperty("history").EnumerateArray().ToArray();
        Assert.Equal(2, history.Length);
        Assert.Equal("done", history[1].GetProperty("event_type").GetString());
    }

    [Fact]
    public async Task DeleteByPublicRefAsync_removes_item_and_appends_deleted_event_with_snapshot()
    {
        var context = CreateContext();
        var repository = CreateRepository(context);
        await repository.AddAsync(CreateItem(
            id: Guid.Parse("0f3a9d94-4df0-47f7-95c1-0f967c22f4db"),
            publicRef: "t-0426-1",
            collection: ItemCollection.Today));

        await repository.DeleteByPublicRefAsync("t-0426-1");

        var found = await repository.FindByPublicRefAsync("t-0426-1");
        Assert.Null(found);

        var json = await File.ReadAllTextAsync(context.MonthlyFilePath);
        using var doc = JsonDocument.Parse(json);
        var items = doc.RootElement.GetProperty("items");
        Assert.Empty(items.EnumerateArray());

        var history = doc.RootElement.GetProperty("history").EnumerateArray().ToArray();
        Assert.Equal(2, history.Length);
        Assert.Equal("deleted", history[1].GetProperty("event_type").GetString());
        var snapshot = history[1].GetProperty("data").GetProperty("snapshot");
        Assert.Equal("t-0426-1", snapshot.GetProperty("public_ref").GetString());

        var indexJson = await File.ReadAllTextAsync(context.IndexFilePath);
        using var indexDoc = JsonDocument.Parse(indexJson);
        Assert.Empty(indexDoc.RootElement.GetProperty("items").EnumerateArray());
    }

    [Fact]
    public async Task ClearHistoryAsync_removes_history_and_keeps_active_items()
    {
        var context = CreateContext();
        var repository = CreateRepository(context);
        var item = CreateItem(
            id: Guid.Parse("0f3a9d94-4df0-47f7-95c1-0f967c22f4db"),
            publicRef: "t-0426-1",
            collection: ItemCollection.Today);
        await repository.AddAsync(item);
        item.MarkDone(ChangedAt);
        await repository.UpdateAsync(item);

        await repository.ClearHistoryAsync();

        var found = await repository.FindByPublicRefAsync("t-0426-1");
        Assert.NotNull(found);

        var json = await File.ReadAllTextAsync(context.MonthlyFilePath);
        using var doc = JsonDocument.Parse(json);
        Assert.Empty(doc.RootElement.GetProperty("history").EnumerateArray());
        Assert.Single(doc.RootElement.GetProperty("items").EnumerateArray());
    }

    [Fact]
    public async Task AddAsync_and_FindByPublicRefAsync_preserve_optional_fields_and_migration_metadata()
    {
        var context = CreateContext(now: new DateTimeOffset(2026, 5, 1, 8, 0, 0, TimeSpan.Zero));
        var repository = CreateRepository(context);
        var migrationInfo = new MigrationInfo(
            "2026-04",
            "2026-05",
            new DateTimeOffset(2026, 5, 1, 8, 0, 0, TimeSpan.Zero),
            "automatic_month_rollover");
        var item = Item.Restore(
            Guid.Parse("0f3a9d94-4df0-47f7-95c1-0f967c22f4db"),
            PublicRef.Parse("t-0426-1"),
            ItemType.Task,
            "Fix authentication flow",
            "keep tests green",
            ItemStatus.Open,
            ItemCollection.Today,
            Priority.High,
            ["auth"],
            3,
            CreatedAt,
            ChangedAt,
            dueAt: new DateTimeOffset(2026, 5, 3, 9, 0, 0, TimeSpan.Zero),
            scheduledAt: new DateTimeOffset(2026, 5, 2, 14, 0, 0, TimeSpan.Zero),
            estimateMinutes: 45,
            migratedAt: migrationInfo.MigratedAt,
            migration: migrationInfo);

        await repository.AddAsync(item);

        var found = await repository.FindByPublicRefAsync("t-0426-1");
        Assert.NotNull(found);
        Assert.Equal(item.Description, found.Description);
        Assert.Equal(item.DueAt, found.DueAt);
        Assert.Equal(item.ScheduledAt, found.ScheduledAt);
        Assert.Equal(item.EstimateMinutes, found.EstimateMinutes);
        Assert.Equal(item.MigratedAt, found.MigratedAt);
        Assert.NotNull(found.Migration);
        Assert.Equal("2026-04", found.Migration!.FromPeriod);
        Assert.Equal("2026-05", found.Migration.ToPeriod);
        Assert.Equal("automatic_month_rollover", found.Migration.Reason);

        var json = await File.ReadAllTextAsync(context.MonthlyFilePath);
        using var doc = JsonDocument.Parse(json);
        var stored = Assert.Single(doc.RootElement.GetProperty("items").EnumerateArray());
        Assert.Equal("keep tests green", stored.GetProperty("description").GetString());
        Assert.Equal(new DateTimeOffset(2026, 5, 3, 9, 0, 0, TimeSpan.Zero), stored.GetProperty("due_at").GetDateTimeOffset());
        Assert.Equal(new DateTimeOffset(2026, 5, 2, 14, 0, 0, TimeSpan.Zero), stored.GetProperty("scheduled_at").GetDateTimeOffset());
        Assert.Equal(45, stored.GetProperty("estimate_minutes").GetInt32());
        Assert.Equal(migrationInfo.MigratedAt, stored.GetProperty("migrated_at").GetDateTimeOffset());
        var migration = stored.GetProperty("migration");
        Assert.Equal("2026-04", migration.GetProperty("from_period").GetString());
        Assert.Equal("2026-05", migration.GetProperty("to_period").GetString());
        Assert.Equal("automatic_month_rollover", migration.GetProperty("reason").GetString());
    }

    [Fact]
    public async Task RunAutomaticMonthRolloverAsync_moves_active_tasks_from_previous_month_to_current_month()
    {
        var context = CreateContext(now: new DateTimeOffset(2026, 5, 1, 8, 0, 0, TimeSpan.Zero));
        var repository = CreateRepository(context);
        Directory.CreateDirectory(Path.GetDirectoryName(context.PreviousMonthlyFilePath)!);
        await File.WriteAllTextAsync(
            context.PreviousMonthlyFilePath,
            """
            {
              "period": "2026-04",
              "file_name": "data_04_2026.json",
              "public_ref_sequences": {
                "task": 1,
                "note": 0,
                "event": 0
              },
              "items": [
                {
                  "id": "0f3a9d94-4df0-47f7-95c1-0f967c22f4db",
                  "public_ref": "t-0426-1",
                  "type": "task",
                  "content": "Fix authentication flow",
                  "description": null,
                  "status": "open",
                  "collection": "today",
                  "priority": "none",
                  "tags": ["auth"],
                  "version": 1,
                  "created_at": "2026-04-23T10:30:00Z",
                  "updated_at": "2026-04-23T10:30:00Z",
                  "completed_at": null,
                  "cancelled_at": null,
                  "migrated_at": null,
                  "migration": null
                }
              ],
              "history": [],
              "settings": {}
            }
            """);

        await repository.RunAutomaticMonthRolloverAsync();

        var previousJson = await File.ReadAllTextAsync(context.PreviousMonthlyFilePath);
        using var previousDoc = JsonDocument.Parse(previousJson);
        Assert.Empty(previousDoc.RootElement.GetProperty("items").EnumerateArray());
        var previousHistory = previousDoc.RootElement.GetProperty("history").EnumerateArray().ToArray();
        Assert.Single(previousHistory);
        Assert.Equal("migrated", previousHistory[0].GetProperty("event_type").GetString());

        Assert.True(File.Exists(context.MonthlyFilePath));
        var currentJson = await File.ReadAllTextAsync(context.MonthlyFilePath);
        using var currentDoc = JsonDocument.Parse(currentJson);
        var migratedItem = Assert.Single(currentDoc.RootElement.GetProperty("items").EnumerateArray());
        Assert.Equal("t-0426-1", migratedItem.GetProperty("public_ref").GetString());
        Assert.Equal("open", migratedItem.GetProperty("status").GetString());
        Assert.Equal(new DateTimeOffset(2026, 5, 1, 8, 0, 0, TimeSpan.Zero), migratedItem.GetProperty("migrated_at").GetDateTimeOffset());
        var migration = migratedItem.GetProperty("migration");
        Assert.Equal("2026-04", migration.GetProperty("from_period").GetString());
        Assert.Equal("2026-05", migration.GetProperty("to_period").GetString());
        Assert.Equal("automatic_month_rollover", migration.GetProperty("reason").GetString());

        var found = await repository.FindByPublicRefAsync("t-0426-1");
        Assert.NotNull(found);
        Assert.NotNull(found.Migration);
        Assert.Equal("2026-04", found.Migration!.FromPeriod);
        Assert.Equal("2026-05", found.Migration.ToPeriod);

        var indexJson = await File.ReadAllTextAsync(context.IndexFilePath);
        using var indexDoc = JsonDocument.Parse(indexJson);
        Assert.Single(indexDoc.RootElement.GetProperty("items").EnumerateArray());
    }

    [Fact]
    public async Task RunAutomaticMonthRolloverAsync_is_idempotent_for_same_startup_day()
    {
        var context = CreateContext(now: new DateTimeOffset(2026, 5, 1, 8, 0, 0, TimeSpan.Zero));
        var repository = CreateRepository(context);
        Directory.CreateDirectory(Path.GetDirectoryName(context.PreviousMonthlyFilePath)!);
        await File.WriteAllTextAsync(
            context.PreviousMonthlyFilePath,
            """
            {
              "period": "2026-04",
              "file_name": "data_04_2026.json",
              "public_ref_sequences": { "task": 1, "note": 0, "event": 0 },
              "items": [
                {
                  "id": "0f3a9d94-4df0-47f7-95c1-0f967c22f4db",
                  "public_ref": "t-0426-1",
                  "type": "task",
                  "content": "Fix authentication flow",
                  "description": null,
                  "status": "open",
                  "collection": "today",
                  "priority": "none",
                  "tags": [],
                  "version": 1,
                  "created_at": "2026-04-23T10:30:00Z",
                  "updated_at": "2026-04-23T10:30:00Z",
                  "completed_at": null,
                  "cancelled_at": null,
                  "migrated_at": null,
                  "migration": null
                }
              ],
              "history": [],
              "settings": {}
            }
            """);

        await repository.RunAutomaticMonthRolloverAsync();
        await repository.RunAutomaticMonthRolloverAsync();

        var currentJson = await File.ReadAllTextAsync(context.MonthlyFilePath);
        using var currentDoc = JsonDocument.Parse(currentJson);
        Assert.Single(currentDoc.RootElement.GetProperty("items").EnumerateArray());
    }

    private static readonly DateTimeOffset CreatedAt = new(2026, 4, 23, 10, 30, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset ChangedAt = new(2026, 4, 23, 12, 0, 0, TimeSpan.Zero);

    private static Item CreateItem(Guid id, string publicRef, ItemCollection collection)
    {
        return Item.Create(
            id,
            PublicRef.Parse(publicRef),
            ItemType.Task,
            "Fix authentication flow",
            collection,
            CreatedAt,
            tags: ["auth"]);
    }

    private static JsonFileItemRepository CreateRepository(TestContext? context = null)
    {
        var current = context ?? CreateContext();
        return new JsonFileItemRepository(
            new FixedClock(current.Now),
            new MonthlyJsonFilePathResolver(current.ProjectRootPath),
            new SafeJsonFileStore(),
            new LocalJsonIndexService(current.ProjectRootPath));
    }

    private static TestContext CreateContext(DateTimeOffset? now = null)
    {
        var currentNow = now ?? CreatedAt;
        var projectRootPath = Path.Combine(
            Path.GetTempPath(),
            "TermBullet.Tests",
            Guid.NewGuid().ToString("N"));
        var monthlyFilePath = Path.Combine(projectRootPath, "data", $"{currentNow.Year:0000}", $"data_{currentNow.Month:00}_{currentNow.Year:0000}.json");
        var previousDate = currentNow.AddMonths(-1);
        var previousMonthlyFilePath = Path.Combine(projectRootPath, "data", $"{previousDate.Year:0000}", $"data_{previousDate.Month:00}_{previousDate.Year:0000}.json");
        Directory.CreateDirectory(Path.GetDirectoryName(monthlyFilePath)!);
        var indexFilePath = Path.Combine(projectRootPath, "data", "index.json");
        return new TestContext(projectRootPath, monthlyFilePath, previousMonthlyFilePath, indexFilePath, currentNow);
    }

    private sealed record TestContext(
        string ProjectRootPath,
        string MonthlyFilePath,
        string PreviousMonthlyFilePath,
        string IndexFilePath,
        DateTimeOffset Now);

    private sealed class FixedClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }
}
