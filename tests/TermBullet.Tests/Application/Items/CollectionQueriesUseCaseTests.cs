using TermBullet.Application.Items;
using TermBullet.Application.Ports;
using TermBullet.Core.Items;
using TermBullet.Core.Refs;

namespace TermBullet.Tests.Application.Items;

public sealed class CollectionQueriesUseCaseTests
{
    private static readonly DateTimeOffset Now = new(2026, 4, 23, 10, 30, 0, TimeSpan.Zero);

    [Fact]
    public async Task Today_query_uses_today_collection_filter()
    {
        var repository = new FakeItemRepository([CreateTask()]);
        var useCase = new GetTodayItemsUseCase(repository);

        _ = await useCase.ExecuteAsync();

        Assert.Equal(ItemCollection.Today, repository.LastCollection);
        Assert.Null(repository.LastStatus);
    }

    [Fact]
    public async Task Week_query_uses_week_collection_filter()
    {
        var repository = new FakeItemRepository([CreateTask()]);
        var useCase = new GetWeekItemsUseCase(repository);

        _ = await useCase.ExecuteAsync();

        Assert.Equal(ItemCollection.Week, repository.LastCollection);
        Assert.Null(repository.LastStatus);
    }

    [Fact]
    public async Task Backlog_query_uses_backlog_collection_filter()
    {
        var repository = new FakeItemRepository([CreateTask()]);
        var useCase = new GetBacklogItemsUseCase(repository);

        _ = await useCase.ExecuteAsync();

        Assert.Equal(ItemCollection.Backlog, repository.LastCollection);
        Assert.Null(repository.LastStatus);
    }

    private static Item CreateTask()
    {
        return Item.Create(
            Guid.Parse("0f3a9d94-4df0-47f7-95c1-0f967c22f4db"),
            PublicRef.Parse("t-0426-1"),
            ItemType.Task,
            "Fix authentication flow",
            ItemCollection.Today,
            Now);
    }

    private sealed class FakeItemRepository(IReadOnlyCollection<Item> items) : IItemRepository
    {
        public ItemCollection? LastCollection { get; private set; }

        public ItemStatus? LastStatus { get; private set; }

        public Task<int> GetCurrentPublicRefSequenceAsync(
            ItemType type,
            int month,
            int year,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<bool> PublicRefExistsAsync(
            string publicRef,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task AddAsync(Item item, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task UpdateAsync(Item item, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task DeleteByPublicRefAsync(string publicRef, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyCollection<Item>> ListAsync(
            ItemCollection? collection = null,
            ItemStatus? status = null,
            CancellationToken cancellationToken = default)
        {
            LastCollection = collection;
            LastStatus = status;
            return Task.FromResult(items);
        }

        public Task<Item?> FindByPublicRefAsync(
            string publicRef,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}
