using TermBullet.Application.Items;
using TermBullet.Application.Ports;
using TermBullet.Core.Items;
using TermBullet.Core.Refs;

namespace TermBullet.Tests.Application.Items;

public sealed class MutateItemUseCaseTests
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 4, 23, 10, 30, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset ChangedAt = new(2026, 4, 23, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Edit_updates_content_description_and_persists_item()
    {
        var repository = new FakeItemRepository(CreateTask());
        var useCase = new EditItemUseCase(repository, new FixedClock(ChangedAt));

        var result = await useCase.ExecuteAsync(new EditItemRequest
        {
            PublicRef = "t-0426-1",
            Content = "  Fix auth refresh flow  ",
            Description = "  Include CLI output.  "
        });

        Assert.Equal("Fix auth refresh flow", result.Content);
        Assert.Equal("Include CLI output.", result.Description);
        Assert.Equal(2, result.Version);
        Assert.Equal(ChangedAt, result.UpdatedAt);

        var updatedItem = Assert.Single(repository.UpdatedItems);
        Assert.Equal("Fix auth refresh flow", updatedItem.Content);
        Assert.Equal("Include CLI output.", updatedItem.Description);
        Assert.Equal("t-0426-1", repository.LastPublicRef);
    }

    [Fact]
    public async Task Mark_done_sets_done_status_and_persists_item()
    {
        var repository = new FakeItemRepository(CreateTask());
        var useCase = new MarkDoneItemUseCase(repository, new FixedClock(ChangedAt));

        var result = await useCase.ExecuteAsync("t-0426-1");

        Assert.Equal(ItemStatus.Done, result.Status);
        Assert.Equal(2, result.Version);
        Assert.Equal(ChangedAt, result.UpdatedAt);

        var updatedItem = Assert.Single(repository.UpdatedItems);
        Assert.Equal(ChangedAt, updatedItem.CompletedAt);
    }

    [Fact]
    public async Task Cancel_sets_cancelled_status_and_persists_item()
    {
        var repository = new FakeItemRepository(CreateTask());
        var useCase = new CancelItemUseCase(repository, new FixedClock(ChangedAt));

        var result = await useCase.ExecuteAsync("t-0426-1");

        Assert.Equal(ItemStatus.Cancelled, result.Status);
        Assert.Equal(2, result.Version);
        Assert.Equal(ChangedAt, result.UpdatedAt);

        var updatedItem = Assert.Single(repository.UpdatedItems);
        Assert.Equal(ChangedAt, updatedItem.CancelledAt);
    }

    [Fact]
    public async Task Move_updates_collection_and_persists_item()
    {
        var repository = new FakeItemRepository(CreateTask());
        var useCase = new MoveItemUseCase(repository, new FixedClock(ChangedAt));

        var result = await useCase.ExecuteAsync(new MoveItemRequest
        {
            PublicRef = "t-0426-1",
            Collection = ItemCollection.Backlog
        });

        Assert.Equal(ItemCollection.Backlog, result.Collection);
        Assert.Equal(2, result.Version);
        Assert.Equal(ChangedAt, result.UpdatedAt);
        Assert.Single(repository.UpdatedItems);
    }

    [Fact]
    public async Task Set_priority_updates_priority_and_persists_item()
    {
        var repository = new FakeItemRepository(CreateTask());
        var useCase = new SetItemPriorityUseCase(repository, new FixedClock(ChangedAt));

        var result = await useCase.ExecuteAsync(new SetItemPriorityRequest
        {
            PublicRef = "t-0426-1",
            Priority = Priority.High
        });

        Assert.Equal(Priority.High, result.Priority);
        Assert.Equal(2, result.Version);
        Assert.Equal(ChangedAt, result.UpdatedAt);
        Assert.Single(repository.UpdatedItems);
    }

    [Fact]
    public async Task Tag_adds_normalized_tag_and_persists_item()
    {
        var repository = new FakeItemRepository(CreateTask());
        var useCase = new TagItemUseCase(repository, new FixedClock(ChangedAt));

        var result = await useCase.ExecuteAsync(new TagItemRequest
        {
            PublicRef = "t-0426-1",
            Tag = "  Auth  "
        });

        Assert.Equal(["Auth"], result.Tags);
        Assert.Equal(2, result.Version);
        Assert.Equal(ChangedAt, result.UpdatedAt);
        Assert.Single(repository.UpdatedItems);
    }

    [Fact]
    public async Task Untag_removes_existing_tag_and_persists_item()
    {
        var repository = new FakeItemRepository(CreateTask(tags: ["auth", "cli"]));
        var useCase = new UntagItemUseCase(repository, new FixedClock(ChangedAt));

        var result = await useCase.ExecuteAsync(new UntagItemRequest
        {
            PublicRef = "t-0426-1",
            Tag = "AUTH"
        });

        Assert.Equal(["cli"], result.Tags);
        Assert.Equal(2, result.Version);
        Assert.Equal(ChangedAt, result.UpdatedAt);
        Assert.Single(repository.UpdatedItems);
    }

    [Fact]
    public async Task Mutation_rejects_invalid_public_ref_before_repository_lookup()
    {
        var repository = new FakeItemRepository(CreateTask());
        var useCase = new MarkDoneItemUseCase(repository, new FixedClock(ChangedAt));

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.ExecuteAsync("x-0426-1"));

        Assert.Contains("Invalid public ref", exception.Message);
        Assert.Null(repository.LastPublicRef);
        Assert.Empty(repository.UpdatedItems);
    }

    [Fact]
    public async Task Mutation_throws_when_item_is_not_found()
    {
        var repository = new FakeItemRepository(null);
        var useCase = new MarkDoneItemUseCase(repository, new FixedClock(ChangedAt));

        var exception = await Assert.ThrowsAsync<ItemNotFoundException>(
            () => useCase.ExecuteAsync("t-0426-1"));

        Assert.Equal("t-0426-1", exception.PublicRef);
        Assert.Empty(repository.UpdatedItems);
    }

    [Fact]
    public async Task Mutation_rejects_terminal_status_change()
    {
        var item = CreateTask();
        item.MarkDone(ChangedAt.AddMinutes(-10));
        var repository = new FakeItemRepository(item);
        var useCase = new CancelItemUseCase(repository, new FixedClock(ChangedAt));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync("t-0426-1"));

        Assert.Contains("terminal", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(repository.UpdatedItems);
    }

    [Fact]
    public async Task Edit_rejects_empty_content()
    {
        var repository = new FakeItemRepository(CreateTask());
        var useCase = new EditItemUseCase(repository, new FixedClock(ChangedAt));

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.ExecuteAsync(new EditItemRequest
            {
                PublicRef = "t-0426-1",
                Content = " "
            }));

        Assert.Equal("content", exception.ParamName);
        Assert.Empty(repository.UpdatedItems);
    }

    [Fact]
    public async Task Move_rejects_invalid_collection()
    {
        var repository = new FakeItemRepository(CreateTask());
        var useCase = new MoveItemUseCase(repository, new FixedClock(ChangedAt));

        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => useCase.ExecuteAsync(new MoveItemRequest
            {
                PublicRef = "t-0426-1",
                Collection = (ItemCollection)99
            }));

        Assert.Equal("collection", exception.ParamName);
        Assert.Empty(repository.UpdatedItems);
    }

    [Fact]
    public async Task Set_priority_rejects_invalid_priority()
    {
        var repository = new FakeItemRepository(CreateTask());
        var useCase = new SetItemPriorityUseCase(repository, new FixedClock(ChangedAt));

        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => useCase.ExecuteAsync(new SetItemPriorityRequest
            {
                PublicRef = "t-0426-1",
                Priority = (Priority)99
            }));

        Assert.Equal("priority", exception.ParamName);
        Assert.Empty(repository.UpdatedItems);
    }

    [Fact]
    public async Task Tag_rejects_empty_tag()
    {
        var repository = new FakeItemRepository(CreateTask());
        var useCase = new TagItemUseCase(repository, new FixedClock(ChangedAt));

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.ExecuteAsync(new TagItemRequest
            {
                PublicRef = "t-0426-1",
                Tag = " "
            }));

        Assert.Equal("tag", exception.ParamName);
        Assert.Empty(repository.UpdatedItems);
    }

    private static Item CreateTask(IReadOnlyCollection<string>? tags = null)
    {
        return Item.Create(
            Guid.Parse("0f3a9d94-4df0-47f7-95c1-0f967c22f4db"),
            PublicRef.Parse("t-0426-1"),
            ItemType.Task,
            "Fix authentication flow",
            ItemCollection.Today,
            CreatedAt,
            tags: tags);
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

        public Task UpdateAsync(Item itemToUpdate, CancellationToken cancellationToken = default)
        {
            UpdatedItems.Add(itemToUpdate);
            return Task.CompletedTask;
        }

        public Task DeleteByPublicRefAsync(string publicRef, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FixedClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }
}
