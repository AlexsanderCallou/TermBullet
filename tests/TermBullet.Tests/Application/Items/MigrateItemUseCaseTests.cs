using TermBullet.Application.Items;
using TermBullet.Domain.Items;
using TermBullet.Domain.Refs;
using TermBullet.Repositories.Interfaces;
using TermBullet.Services.Clock;

namespace TermBullet.Tests.Application.Items;

public sealed class MigrateItemUseCaseTests
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 4, 23, 10, 30, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset ChangedAt = new(2026, 5, 1, 0, 5, 0, TimeSpan.Zero);

    [Fact]
    public async Task Execute_moves_same_task_to_destination_collection()
    {
        var repository = new FakeItemRepository(CreateTask(ItemCollection.Backlog));
        var useCase = new MigrateItemUseCase(repository, new FixedClock(ChangedAt));

        var result = await useCase.ExecuteAsync(new MigrateItemRequest
        {
            PublicRef = "t-0426-1",
            DestinationCollection = ItemCollection.Today
        });

        Assert.Equal("t-0426-1", result.PublicRef);
        Assert.Equal(ItemStatus.Open, result.Status);
        Assert.Equal(ItemCollection.Today, result.Collection);
        Assert.Equal(2, result.Version);
        Assert.Equal(ChangedAt, result.UpdatedAt);

        var updatedItem = Assert.Single(repository.UpdatedItems);
        Assert.Equal(Guid.Parse("0f3a9d94-4df0-47f7-95c1-0f967c22f4db"), updatedItem.Id);
        Assert.Equal("t-0426-1", updatedItem.PublicRef.Value);
        Assert.Equal(ItemCollection.Today, updatedItem.Collection);
        Assert.Empty(repository.AddedItems);
    }

    [Theory]
    [InlineData(ItemCollection.Today)]
    [InlineData(ItemCollection.Week)]
    [InlineData(ItemCollection.Month)]
    [InlineData(ItemCollection.Backlog)]
    public async Task Execute_supports_all_task_collections(ItemCollection destination)
    {
        var repository = new FakeItemRepository(CreateTask(ItemCollection.Today));
        var useCase = new MigrateItemUseCase(repository, new FixedClock(ChangedAt));

        var result = await useCase.ExecuteAsync(new MigrateItemRequest
        {
            PublicRef = "t-0426-1",
            DestinationCollection = destination
        });

        Assert.Equal(destination, result.Collection);
        Assert.Equal(ItemStatus.Open, result.Status);
        Assert.Empty(repository.AddedItems);
    }

    [Theory]
    [InlineData(ItemType.Note)]
    [InlineData(ItemType.Event)]
    public async Task Execute_rejects_non_task_items(ItemType type)
    {
        var repository = new FakeItemRepository(CreateItem(type));
        var useCase = new MigrateItemUseCase(repository, new FixedClock(ChangedAt));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(new MigrateItemRequest
            {
                PublicRef = type == ItemType.Note ? "n-0426-1" : "e-0426-1",
                DestinationCollection = ItemCollection.Week
            }));

        Assert.Empty(repository.UpdatedItems);
        Assert.Empty(repository.AddedItems);
    }

    [Fact]
    public async Task Execute_throws_when_item_is_not_found()
    {
        var repository = new FakeItemRepository(null);
        var useCase = new MigrateItemUseCase(repository, new FixedClock(ChangedAt));

        var exception = await Assert.ThrowsAsync<ItemNotFoundException>(
            () => useCase.ExecuteAsync("t-0426-1"));

        Assert.Equal("t-0426-1", exception.PublicRef);
        Assert.Empty(repository.UpdatedItems);
    }

    [Fact]
    public async Task Execute_rejects_invalid_public_ref()
    {
        var repository = new FakeItemRepository(CreateTask(ItemCollection.Today));
        var useCase = new MigrateItemUseCase(repository, new FixedClock(ChangedAt));

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.ExecuteAsync("x-0426-1"));

        Assert.Contains("Invalid public ref", exception.Message);
        Assert.Null(repository.LastPublicRef);
        Assert.Empty(repository.UpdatedItems);
    }

    private static Item CreateTask(ItemCollection collection) =>
        Item.Create(
            Guid.Parse("0f3a9d94-4df0-47f7-95c1-0f967c22f4db"),
            PublicRef.Parse("t-0426-1"),
            ItemType.Task,
            "Fix authentication flow",
            collection,
            CreatedAt);

    private static Item CreateItem(ItemType type)
    {
        var publicRef = type switch
        {
            ItemType.Note => PublicRef.Parse("n-0426-1"),
            ItemType.Event => PublicRef.Parse("e-0426-1"),
            _ => PublicRef.Parse("t-0426-1")
        };

        return Item.Create(
            Guid.NewGuid(),
            publicRef,
            type,
            "Reference item",
            ItemCollection.Backlog,
            CreatedAt);
    }

    private sealed class FakeItemRepository(Item? item) : IItemRepository
    {
        public string? LastPublicRef { get; private set; }

        public List<Item> UpdatedItems { get; } = [];

        public List<Item> AddedItems { get; } = [];

        public Task<int> GetCurrentPublicRefSequenceAsync(
            ItemType type,
            int month,
            int year,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(0);

        public Task<bool> PublicRefExistsAsync(
            string publicRef,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task AddAsync(Item itemToAdd, CancellationToken cancellationToken = default)
        {
            AddedItems.Add(itemToAdd);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Item itemToUpdate, CancellationToken cancellationToken = default)
        {
            UpdatedItems.Add(itemToUpdate);
            return Task.CompletedTask;
        }

        public Task DeleteByPublicRefAsync(string publicRef, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task ClearHistoryAsync(CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<IReadOnlyCollection<Item>> ListAsync(
            ItemCollection? collection = null,
            ItemStatus? status = null,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<Item?> FindByPublicRefAsync(
            string publicRef,
            CancellationToken cancellationToken = default)
        {
            LastPublicRef = publicRef;
            return Task.FromResult(item);
        }
    }

    private sealed class FixedClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }
}
