using TermBullet.Application.Items;
using TermBullet.Application.Ports;
using TermBullet.Core.Items;
using TermBullet.Core.Refs;

namespace TermBullet.Tests.Application.Items;

public sealed class ListItemsUseCaseTests
{
    private static readonly DateTimeOffset Now = new(2026, 4, 23, 10, 30, 0, TimeSpan.Zero);

    [Fact]
    public async Task Execute_returns_all_items_when_no_filter_is_provided()
    {
        var repository = new FakeItemRepository([CreateTask(), CreateNote()]);
        var useCase = new ListItemsUseCase(repository);

        var results = await useCase.ExecuteAsync(new ListItemsRequest());

        Assert.Equal(["t-0426-1", "n-0426-1"], results.Select(item => item.PublicRef));
        Assert.Null(repository.LastCollection);
        Assert.Null(repository.LastStatus);
    }

    [Fact]
    public async Task Execute_filters_by_collection()
    {
        var repository = new FakeItemRepository(
            [
                CreateTask(collection: ItemCollection.Today),
                CreateNote(collection: ItemCollection.Backlog)
            ]);
        var useCase = new ListItemsUseCase(repository);

        var results = await useCase.ExecuteAsync(new ListItemsRequest
        {
            Collection = ItemCollection.Backlog
        });

        var result = Assert.Single(results);
        Assert.Equal("n-0426-1", result.PublicRef);
        Assert.Equal(ItemCollection.Backlog, repository.LastCollection);
        Assert.Null(repository.LastStatus);
    }

    [Fact]
    public async Task Execute_filters_by_status()
    {
        var doneTask = CreateTask();
        doneTask.MarkDone(Now.AddHours(1));
        var repository = new FakeItemRepository([CreateNote(), doneTask]);
        var useCase = new ListItemsUseCase(repository);

        var results = await useCase.ExecuteAsync(new ListItemsRequest
        {
            Status = ItemStatus.Done
        });

        var result = Assert.Single(results);
        Assert.Equal("t-0426-1", result.PublicRef);
        Assert.Equal(ItemStatus.Done, repository.LastStatus);
        Assert.Null(repository.LastCollection);
    }

    [Fact]
    public async Task Execute_rejects_invalid_collection_filter()
    {
        var repository = new FakeItemRepository([]);
        var useCase = new ListItemsUseCase(repository);

        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => useCase.ExecuteAsync(new ListItemsRequest
            {
                Collection = (ItemCollection)99
            }));

        Assert.Equal("collection", exception.ParamName);
        Assert.Equal(0, repository.ListCallCount);
    }

    [Fact]
    public async Task Execute_rejects_invalid_status_filter()
    {
        var repository = new FakeItemRepository([]);
        var useCase = new ListItemsUseCase(repository);

        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => useCase.ExecuteAsync(new ListItemsRequest
            {
                Status = (ItemStatus)99
            }));

        Assert.Equal("status", exception.ParamName);
        Assert.Equal(0, repository.ListCallCount);
    }

    private static Item CreateTask(ItemCollection collection = ItemCollection.Today)
    {
        return Item.Create(
            Guid.Parse("0f3a9d94-4df0-47f7-95c1-0f967c22f4db"),
            PublicRef.Parse("t-0426-1"),
            ItemType.Task,
            "Fix authentication flow",
            collection,
            Now);
    }

    private static Item CreateNote(ItemCollection collection = ItemCollection.Today)
    {
        return Item.Create(
            Guid.Parse("4bb0d4e0-44f3-46fe-9391-f3c01321c46d"),
            PublicRef.Parse("n-0426-1"),
            ItemType.Note,
            "Review monthly goals",
            collection,
            Now);
    }

    private sealed class FakeItemRepository(IReadOnlyCollection<Item> items) : IItemRepository
    {
        public ItemCollection? LastCollection { get; private set; }

        public ItemStatus? LastStatus { get; private set; }

        public int ListCallCount { get; private set; }

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
            ListCallCount++;
            LastCollection = collection;
            LastStatus = status;

            var results = items
                .Where(item => collection is null || item.Collection == collection)
                .Where(item => status is null || item.Status == status)
                .ToArray();

            return Task.FromResult<IReadOnlyCollection<Item>>(results);
        }

        public Task<Item?> FindByPublicRefAsync(
            string publicRef,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}
