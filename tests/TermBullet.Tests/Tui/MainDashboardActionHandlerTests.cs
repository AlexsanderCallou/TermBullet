using TermBullet.Application.Items;
using TermBullet.Application.Ports;
using TermBullet.Core.Items;
using TermBullet.Core.Refs;
using TermBullet.Tui.Screens;

namespace TermBullet.Tests.Tui;

public sealed class MainDashboardActionHandlerTests
{
    [Fact]
    public async Task HandleDoneAsync_marks_item_as_done()
    {
        var repo = new FakeItemRepository();
        repo.Add(MakeItem("t-0426-1"));
        var handler = CreateHandler(repo);

        var result = await handler.HandleDoneAsync("t-0426-1");

        Assert.True(result.Success);
        Assert.Equal(ItemStatus.Done, repo.GetByRef("t-0426-1")!.Status);
    }

    [Fact]
    public async Task HandleDoneAsync_returns_failure_when_ref_not_found()
    {
        var repo = new FakeItemRepository();
        var handler = CreateHandler(repo);

        var result = await handler.HandleDoneAsync("t-0426-99");

        Assert.False(result.Success);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
    }

    [Fact]
    public async Task HandleCancelAsync_cancels_item()
    {
        var repo = new FakeItemRepository();
        repo.Add(MakeItem("t-0426-1"));
        var handler = CreateHandler(repo);

        var result = await handler.HandleCancelAsync("t-0426-1");

        Assert.True(result.Success);
        Assert.Equal(ItemStatus.Cancelled, repo.GetByRef("t-0426-1")!.Status);
    }

    [Fact]
    public async Task HandleDeleteAsync_removes_item_from_repository()
    {
        var repo = new FakeItemRepository();
        repo.Add(MakeItem("t-0426-1"));
        var handler = CreateHandler(repo);

        var result = await handler.HandleDeleteAsync("t-0426-1");

        Assert.True(result.Success);
        Assert.Null(repo.GetByRef("t-0426-1"));
    }

    [Fact]
    public async Task HandleMigrateAsync_marks_item_as_migrated()
    {
        var repo = new FakeItemRepository();
        repo.Add(MakeItem("t-0426-1"));
        var handler = CreateHandler(repo);

        var result = await handler.HandleMigrateAsync("t-0426-1");

        Assert.True(result.Success);
        Assert.Equal(ItemStatus.Migrated, repo.GetByRef("t-0426-1")!.Status);
    }

    private static MainDashboardActionHandler CreateHandler(FakeItemRepository repo)
    {
        var clock = new FixedClock(new DateTimeOffset(2026, 4, 24, 9, 0, 0, TimeSpan.Zero));
        return new MainDashboardActionHandler(
            new MarkDoneItemUseCase(repo, clock),
            new CancelItemUseCase(repo, clock),
            new MigrateItemUseCase(repo, clock),
            new DeleteItemUseCase(repo));
    }

    private static Item MakeItem(string publicRef) =>
        Item.Create(
            id: Guid.NewGuid(),
            publicRef: PublicRef.Parse(publicRef),
            type: ItemType.Task,
            content: "Test item",
            collection: ItemCollection.Today,
            createdAt: DateTimeOffset.UtcNow);

    private sealed class FixedClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }

    private sealed class FakeItemRepository : IItemRepository
    {
        private readonly Dictionary<string, Item> _items = new(StringComparer.OrdinalIgnoreCase);

        public void Add(Item item) => _items[item.PublicRef.Value] = item;

        public Item? GetByRef(string publicRef) =>
            _items.TryGetValue(publicRef, out var item) ? item : null;

        public Task<Item?> FindByPublicRefAsync(string publicRef, CancellationToken cancellationToken = default) =>
            Task.FromResult(GetByRef(publicRef));

        public Task AddAsync(Item item, CancellationToken cancellationToken = default)
        {
            _items[item.PublicRef.Value] = item;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Item item, CancellationToken cancellationToken = default)
        {
            _items[item.PublicRef.Value] = item;
            return Task.CompletedTask;
        }

        public Task DeleteByPublicRefAsync(string publicRef, CancellationToken cancellationToken = default)
        {
            _items.Remove(publicRef);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<Item>> ListAsync(
            ItemCollection? collection = null,
            ItemStatus? status = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<Item>>(_items.Values.ToList());

        public Task<bool> PublicRefExistsAsync(string publicRef, CancellationToken cancellationToken = default) =>
            Task.FromResult(_items.ContainsKey(publicRef));

        public Task<int> GetCurrentPublicRefSequenceAsync(
            ItemType type, int month, int year, CancellationToken cancellationToken = default) =>
            Task.FromResult(_items.Count);

        public Task ClearHistoryAsync(CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
