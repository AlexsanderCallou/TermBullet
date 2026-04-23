using TermBullet.Application.Items;
using TermBullet.Application.Ports;
using TermBullet.Core.Items;
using TermBullet.Core.Refs;

namespace TermBullet.Tests.Application.Items;

public sealed class MigrateItemUseCaseTests
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 4, 23, 10, 30, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset ChangedAt = new(2026, 5, 1, 0, 5, 0, TimeSpan.Zero);

    [Fact]
    public async Task Execute_sets_migrated_status_and_persists_item()
    {
        var repository = new FakeItemRepository(CreateTask());
        var useCase = new MigrateItemUseCase(repository, new FixedClock(ChangedAt));

        var result = await useCase.ExecuteAsync("t-0426-1");

        Assert.Equal(ItemStatus.Migrated, result.Status);
        Assert.Equal(2, result.Version);
        Assert.Equal(ChangedAt, result.UpdatedAt);

        var updatedItem = Assert.Single(repository.UpdatedItems);
        Assert.Equal(ChangedAt, updatedItem.MigratedAt);
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
        var repository = new FakeItemRepository(CreateTask());
        var useCase = new MigrateItemUseCase(repository, new FixedClock(ChangedAt));

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.ExecuteAsync("x-0426-1"));

        Assert.Contains("Invalid public ref", exception.Message);
        Assert.Null(repository.LastPublicRef);
        Assert.Empty(repository.UpdatedItems);
    }

    private static Item CreateTask()
    {
        return Item.Create(
            Guid.Parse("0f3a9d94-4df0-47f7-95c1-0f967c22f4db"),
            PublicRef.Parse("t-0426-1"),
            ItemType.Task,
            "Fix authentication flow",
            ItemCollection.Today,
            CreatedAt);
    }

    private sealed class FakeItemRepository(Item? item) : IItemRepository
    {
        public string? LastPublicRef { get; private set; }

        public List<Item> UpdatedItems { get; } = [];

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

        public Task AddAsync(Item itemToAdd, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task UpdateAsync(Item itemToUpdate, CancellationToken cancellationToken = default)
        {
            UpdatedItems.Add(itemToUpdate);
            return Task.CompletedTask;
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
            throw new NotSupportedException();
        }

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
