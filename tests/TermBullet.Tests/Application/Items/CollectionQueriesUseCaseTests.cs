using TermBullet.Application.Items;
using TermBullet.Services.Clock;
using TermBullet.Repositories.Interfaces;
using TermBullet.Domain.Items;
using TermBullet.Domain.Refs;

namespace TermBullet.Tests.Application.Items;

public sealed class CollectionQueriesUseCaseTests
{
    private static readonly DateTimeOffset Now = new(2026, 4, 23, 10, 30, 0, TimeSpan.Zero);

    [Fact]
    public async Task Today_query_uses_today_collection_filter()
    {
        var repository = new FakeItemRepository([CreateTask()]);
        var useCase = new GetTodayItemsUseCase(repository, new FixedClock(Now));

        _ = await useCase.ExecuteAsync();

        Assert.Equal(ItemCollection.Today, repository.LastCollection);
        Assert.Null(repository.LastStatus);
    }

    [Fact]
    public async Task Today_query_keeps_terminal_tasks_only_on_their_terminal_local_date()
    {
        var todayDone = CreateTask(
            "t-0426-1",
            ItemStatus.Done,
            completedAt: Now.AddHours(-1),
            updatedAt: Now.AddHours(-1));
        var oldDone = CreateTask(
            "t-0426-2",
            ItemStatus.Done,
            completedAt: Now.AddDays(-1),
            updatedAt: Now.AddDays(-1));
        var todayCancelled = CreateTask(
            "t-0426-3",
            ItemStatus.Cancelled,
            cancelledAt: Now.AddMinutes(-30),
            updatedAt: Now.AddMinutes(-30));
        var oldCancelled = CreateTask(
            "t-0426-4",
            ItemStatus.Cancelled,
            cancelledAt: Now.AddDays(-1),
            updatedAt: Now.AddDays(-1));
        var open = CreateTask("t-0426-5", ItemStatus.Open);
        var repository = new FakeItemRepository([todayDone, oldDone, todayCancelled, oldCancelled, open]);
        var useCase = new GetTodayItemsUseCase(repository, new FixedClock(Now));

        var result = await useCase.ExecuteAsync();

        Assert.Contains(result, item => item.PublicRef == "t-0426-1");
        Assert.DoesNotContain(result, item => item.PublicRef == "t-0426-2");
        Assert.Contains(result, item => item.PublicRef == "t-0426-3");
        Assert.DoesNotContain(result, item => item.PublicRef == "t-0426-4");
        Assert.Contains(result, item => item.PublicRef == "t-0426-5");
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
        return CreateTask("t-0426-1", ItemStatus.Open);
    }

    private static Item CreateTask(
        string publicRef,
        ItemStatus status,
        DateTimeOffset? completedAt = null,
        DateTimeOffset? cancelledAt = null,
        DateTimeOffset? updatedAt = null)
    {
        return Item.Restore(
            Guid.NewGuid(),
            PublicRef.Parse(publicRef),
            ItemType.Task,
            "Fix authentication flow",
            null,
            status,
            ItemCollection.Today,
            Priority.None,
            Item.DefaultTag,
            1,
            Now.AddDays(-3),
            updatedAt ?? Now.AddDays(-3),
            completedAt: completedAt,
            cancelledAt: cancelledAt);
    }

    private sealed class FixedClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
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

        public Task ClearHistoryAsync(CancellationToken cancellationToken = default)
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
