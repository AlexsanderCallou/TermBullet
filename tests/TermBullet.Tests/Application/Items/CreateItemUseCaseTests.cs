using TermBullet.Application.Items;
using TermBullet.Application.Ports;
using TermBullet.Core.Items;

namespace TermBullet.Tests.Application.Items;

public sealed class CreateItemUseCaseTests
{
    private static readonly Guid ItemId = Guid.Parse("0f3a9d94-4df0-47f7-95c1-0f967c22f4db");
    private static readonly DateTimeOffset Now = new(2026, 4, 23, 10, 30, 0, TimeSpan.Zero);

    [Fact]
    public async Task Execute_creates_item_with_generated_public_ref_and_persists_it()
    {
        var repository = new FakeItemRepository { CurrentSequence = 2 };
        var useCase = CreateUseCase(repository);
        var request = new CreateItemRequest
        {
            Type = ItemType.Task,
            Content = "  Fix authentication flow  ",
            Collection = ItemCollection.Today,
            Priority = Priority.High,
            Description = "  Keep CLI and TUI behavior aligned.  ",
            Tags = ["auth", "cli"]
        };

        var result = await useCase.ExecuteAsync(request);

        Assert.Equal(ItemId, result.Id);
        Assert.Equal("t-0426-3", result.PublicRef);
        Assert.Equal(ItemType.Task, result.Type);
        Assert.Equal(ItemStatus.Open, result.Status);
        Assert.Equal(ItemCollection.Today, result.Collection);
        Assert.Equal(Priority.High, result.Priority);
        Assert.Equal(1, result.Version);
        Assert.Equal(Now, result.CreatedAt);

        var sequenceRequest = Assert.Single(repository.SequenceRequests);
        Assert.Equal((ItemType.Task, 4, 2026), sequenceRequest);

        var item = Assert.Single(repository.AddedItems);
        Assert.Equal(ItemId, item.Id);
        Assert.Equal("t-0426-3", item.PublicRef.Value);
        Assert.Equal("Fix authentication flow", item.Content);
        Assert.Equal("Keep CLI and TUI behavior aligned.", item.Description);
        Assert.Equal(["auth", "cli"], item.Tags);
    }

    [Fact]
    public async Task Execute_defaults_collection_and_priority()
    {
        var repository = new FakeItemRepository();
        var useCase = CreateUseCase(repository);
        var request = new CreateItemRequest
        {
            Type = ItemType.Note,
            Content = "Review monthly goals"
        };

        var result = await useCase.ExecuteAsync(request);

        Assert.Equal("n-0426-1", result.PublicRef);
        Assert.Equal(ItemCollection.Today, result.Collection);
        Assert.Equal(Priority.None, result.Priority);
    }

    [Fact]
    public async Task Execute_rejects_duplicate_generated_public_ref()
    {
        var repository = new FakeItemRepository();
        repository.ExistingPublicRefs.Add("t-0426-1");
        var useCase = CreateUseCase(repository);
        var request = new CreateItemRequest
        {
            Type = ItemType.Task,
            Content = "Fix authentication flow"
        };

        var exception = await Assert.ThrowsAsync<DuplicatePublicRefException>(
            () => useCase.ExecuteAsync(request));

        Assert.Equal("t-0426-1", exception.PublicRef);
        Assert.Empty(repository.AddedItems);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public async Task Execute_rejects_empty_content(string content)
    {
        var repository = new FakeItemRepository();
        var useCase = CreateUseCase(repository);
        var request = new CreateItemRequest
        {
            Type = ItemType.Task,
            Content = content
        };

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.ExecuteAsync(request));

        Assert.Equal("content", exception.ParamName);
        Assert.Empty(repository.AddedItems);
    }

    [Fact]
    public async Task Execute_rejects_invalid_item_type()
    {
        var repository = new FakeItemRepository();
        var useCase = CreateUseCase(repository);
        var request = new CreateItemRequest
        {
            Type = (ItemType)99,
            Content = "Fix authentication flow"
        };

        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => useCase.ExecuteAsync(request));

        Assert.Equal("type", exception.ParamName);
        Assert.Empty(repository.AddedItems);
    }

    [Fact]
    public async Task Execute_rejects_invalid_collection()
    {
        var repository = new FakeItemRepository();
        var useCase = CreateUseCase(repository);
        var request = new CreateItemRequest
        {
            Type = ItemType.Task,
            Content = "Fix authentication flow",
            Collection = (ItemCollection)99
        };

        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => useCase.ExecuteAsync(request));

        Assert.Equal("collection", exception.ParamName);
        Assert.Empty(repository.AddedItems);
    }

    [Fact]
    public async Task Execute_rejects_invalid_priority()
    {
        var repository = new FakeItemRepository();
        var useCase = CreateUseCase(repository);
        var request = new CreateItemRequest
        {
            Type = ItemType.Task,
            Content = "Fix authentication flow",
            Priority = (Priority)99
        };

        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => useCase.ExecuteAsync(request));

        Assert.Equal("priority", exception.ParamName);
        Assert.Empty(repository.AddedItems);
    }

    [Fact]
    public async Task Execute_rejects_empty_tag()
    {
        var repository = new FakeItemRepository();
        var useCase = CreateUseCase(repository);
        var request = new CreateItemRequest
        {
            Type = ItemType.Task,
            Content = "Fix authentication flow",
            Tags = ["auth", " "]
        };

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.ExecuteAsync(request));

        Assert.Equal("tags", exception.ParamName);
        Assert.Empty(repository.AddedItems);
    }

    private static CreateItemUseCase CreateUseCase(FakeItemRepository repository)
    {
        return new CreateItemUseCase(
            repository,
            new FixedClock(Now),
            new FixedIdGenerator(ItemId));
    }

    private sealed class FakeItemRepository : IItemRepository
    {
        public int CurrentSequence { get; init; }

        public List<(ItemType Type, int Month, int Year)> SequenceRequests { get; } = [];

        public HashSet<string> ExistingPublicRefs { get; } = [];

        public List<Item> AddedItems { get; } = [];

        public Task<int> GetCurrentPublicRefSequenceAsync(
            ItemType type,
            int month,
            int year,
            CancellationToken cancellationToken = default)
        {
            SequenceRequests.Add((type, month, year));
            return Task.FromResult(CurrentSequence);
        }

        public Task<bool> PublicRefExistsAsync(
            string publicRef,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ExistingPublicRefs.Contains(publicRef));
        }

        public Task AddAsync(Item item, CancellationToken cancellationToken = default)
        {
            AddedItems.Add(item);
            ExistingPublicRefs.Add(item.PublicRef.Value);
            return Task.CompletedTask;
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
            throw new NotSupportedException();
        }

        public Task<Item?> FindByPublicRefAsync(
            string publicRef,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FixedClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }

    private sealed class FixedIdGenerator(Guid id) : IIdGenerator
    {
        public Guid NewId() => id;
    }
}
