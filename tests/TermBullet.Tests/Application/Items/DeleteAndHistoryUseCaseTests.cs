using TermBullet.Application.Items;
using TermBullet.Application.Ports;
using TermBullet.Core.Items;

namespace TermBullet.Tests.Application.Items;

public sealed class DeleteAndHistoryUseCaseTests
{
    [Fact]
    public async Task DeleteItemUseCase_deletes_existing_item_by_public_ref()
    {
        var repository = new FakeItemRepository(exists: true);
        var useCase = new DeleteItemUseCase(repository);

        await useCase.ExecuteAsync("t-0426-1");

        Assert.Equal("t-0426-1", repository.DeletedPublicRef);
    }

    [Fact]
    public async Task DeleteItemUseCase_throws_when_item_is_not_found()
    {
        var repository = new FakeItemRepository(exists: false);
        var useCase = new DeleteItemUseCase(repository);

        var exception = await Assert.ThrowsAsync<ItemNotFoundException>(
            () => useCase.ExecuteAsync("t-0426-1"));

        Assert.Equal("t-0426-1", exception.PublicRef);
    }

    [Fact]
    public async Task ClearHistoryUseCase_clears_history_for_current_month()
    {
        var repository = new FakeItemRepository(exists: true);
        var useCase = new ClearHistoryUseCase(repository);

        await useCase.ExecuteAsync();

        Assert.True(repository.ClearHistoryCalled);
    }

    private sealed class FakeItemRepository(bool exists) : IItemRepository
    {
        public string? DeletedPublicRef { get; private set; }

        public bool ClearHistoryCalled { get; private set; }

        public Task<int> GetCurrentPublicRefSequenceAsync(ItemType type, int month, int year, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<bool> PublicRefExistsAsync(string publicRef, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task AddAsync(Item item, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task UpdateAsync(Item item, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task DeleteByPublicRefAsync(string publicRef, CancellationToken cancellationToken = default)
        {
            if (!exists)
            {
                throw new KeyNotFoundException();
            }

            DeletedPublicRef = publicRef;
            return Task.CompletedTask;
        }

        public Task ClearHistoryAsync(CancellationToken cancellationToken = default)
        {
            ClearHistoryCalled = true;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<Item>> ListAsync(ItemCollection? collection = null, ItemStatus? status = null, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Item?> FindByPublicRefAsync(string publicRef, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
