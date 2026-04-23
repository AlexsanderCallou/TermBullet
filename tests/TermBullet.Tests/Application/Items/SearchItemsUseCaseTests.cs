using TermBullet.Application.Items;
using TermBullet.Application.Ports;
using TermBullet.Core.Items;
using TermBullet.Core.Refs;

namespace TermBullet.Tests.Application.Items;

public sealed class SearchItemsUseCaseTests
{
    private static readonly DateTimeOffset Now = new(2026, 4, 23, 10, 30, 0, TimeSpan.Zero);

    [Fact]
    public async Task Execute_returns_empty_when_nothing_matches()
    {
        var repository = new FakeItemRepository(
            [
                CreateTask(sequence: 1, "Fix authentication flow", "auth"),
                CreateNote(sequence: 1, "Review monthly goals", "planning")
            ]);
        var useCase = new SearchItemsUseCase(repository);

        var results = await useCase.ExecuteAsync(new SearchItemsRequest { Query = "database" });

        Assert.Empty(results);
    }

    [Fact]
    public async Task Execute_returns_multiple_items_for_matching_query()
    {
        var repository = new FakeItemRepository(
            [
                CreateTask(sequence: 1, "Fix authentication flow", "auth"),
                CreateTask(sequence: 2, "Write auth integration tests", "tests"),
                CreateNote(sequence: 1, "Review monthly goals", "planning")
            ]);
        var useCase = new SearchItemsUseCase(repository);

        var results = await useCase.ExecuteAsync(new SearchItemsRequest { Query = "auth" });

        Assert.Equal(["t-0426-1", "t-0426-2"], results.Select(item => item.PublicRef));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public async Task Execute_rejects_empty_query(string query)
    {
        var repository = new FakeItemRepository([]);
        var useCase = new SearchItemsUseCase(repository);

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.ExecuteAsync(new SearchItemsRequest { Query = query }));

        Assert.Equal("query", exception.ParamName);
        Assert.Equal(0, repository.ListCallCount);
    }

    private static Item CreateTask(int sequence, string content, string tag)
    {
        return Item.Create(
            Guid.NewGuid(),
            PublicRef.Create(ItemType.Task, 4, 2026, sequence: sequence),
            ItemType.Task,
            content,
            ItemCollection.Today,
            Now,
            tags: [tag]);
    }

    private static Item CreateNote(int sequence, string content, string tag)
    {
        return Item.Create(
            Guid.NewGuid(),
            PublicRef.Create(ItemType.Note, 4, 2026, sequence: sequence),
            ItemType.Note,
            content,
            ItemCollection.Today,
            Now,
            tags: [tag]);
    }

    private sealed class FakeItemRepository(IReadOnlyCollection<Item> items) : IItemRepository
    {
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

        public Task ClearHistoryAsync(CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyCollection<Item>> ListAsync(
            ItemCollection? collection = null,
            ItemStatus? status = null,
            CancellationToken cancellationToken = default)
        {
            ListCallCount++;
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
