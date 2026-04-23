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
        var repository = CreateRepository();
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
            new FixedClock(CreatedAt),
            new MonthlyJsonFilePathResolver(current.ProjectRootPath),
            new SafeJsonFileStore());
    }

    private static TestContext CreateContext()
    {
        var projectRootPath = Path.Combine(
            Path.GetTempPath(),
            "TermBullet.Tests",
            Guid.NewGuid().ToString("N"));
        var monthlyFilePath = Path.Combine(projectRootPath, "data", "2026", "data_04_2026.json");
        Directory.CreateDirectory(Path.GetDirectoryName(monthlyFilePath)!);
        return new TestContext(projectRootPath, monthlyFilePath);
    }

    private sealed record TestContext(string ProjectRootPath, string MonthlyFilePath);

    private sealed class FixedClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }
}
