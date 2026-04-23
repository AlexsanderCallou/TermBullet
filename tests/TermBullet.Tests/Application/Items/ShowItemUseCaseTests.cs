using TermBullet.Application.Items;
using TermBullet.Application.Ports;
using TermBullet.Core.Items;
using TermBullet.Core.Refs;

namespace TermBullet.Tests.Application.Items;

public sealed class ShowItemUseCaseTests
{
    private static readonly DateTimeOffset Now = new(2026, 4, 23, 10, 30, 0, TimeSpan.Zero);

    [Fact]
    public async Task Execute_returns_item_for_public_ref()
    {
        var item = Item.Create(
            Guid.Parse("0f3a9d94-4df0-47f7-95c1-0f967c22f4db"),
            PublicRef.Parse("t-0426-1"),
            ItemType.Task,
            "Fix authentication flow",
            ItemCollection.Today,
            Now,
            tags: ["auth"]);
        var repository = new FakeItemRepository(item);
        var useCase = new ShowItemUseCase(repository);

        var result = await useCase.ExecuteAsync("t-0426-1");

        Assert.Equal(item.Id, result.Id);
        Assert.Equal("t-0426-1", result.PublicRef);
        Assert.Equal(ItemType.Task, result.Type);
        Assert.Equal("Fix authentication flow", result.Content);
        Assert.Equal(ItemStatus.Open, result.Status);
        Assert.Equal(["auth"], result.Tags);
        Assert.Equal("t-0426-1", repository.LastPublicRef);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("x-0426-1")]
    [InlineData("t-1326-1")]
    public async Task Execute_rejects_invalid_public_ref(string publicRef)
    {
        var repository = new FakeItemRepository(null);
        var useCase = new ShowItemUseCase(repository);

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.ExecuteAsync(publicRef));

        Assert.Contains("Invalid public ref", exception.Message);
        Assert.Null(repository.LastPublicRef);
    }

    [Fact]
    public async Task Execute_throws_when_item_is_not_found()
    {
        var repository = new FakeItemRepository(null);
        var useCase = new ShowItemUseCase(repository);

        var exception = await Assert.ThrowsAsync<ItemNotFoundException>(
            () => useCase.ExecuteAsync("t-0426-99"));

        Assert.Equal("t-0426-99", exception.PublicRef);
    }

    private sealed class FakeItemRepository(Item? item) : IItemRepository
    {
        public string? LastPublicRef { get; private set; }

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
}
