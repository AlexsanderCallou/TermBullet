using System.Text.Json;
using TermBullet.Application.Items;
using TermBullet.Domain.Items;
using TermBullet.Domain.Refs;
using TermBullet.Repositories.Interfaces;
using TermBullet.Services.Clock;

namespace TermBullet.Tests.Application.Items;

public sealed class DailyReviewUseCaseTests
{
    private static readonly DateTimeOffset Now = new(2026, 4, 23, 10, 30, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Yesterday = Now.AddDays(-1);

    [Fact]
    public async Task GetDailyReviewItemsAsync_returns_open_today_tasks_last_reviewed_before_today()
    {
        var stale = CreateTask("t-0426-1", "Stale today task", ItemStatus.Open);
        var reviewedToday = CreateTask("t-0426-2", "Reviewed today task", ItemStatus.Open);
        var done = CreateTask("t-0426-3", "Done task", ItemStatus.Done, completedAt: Yesterday, updatedAt: Yesterday);
        var repository = new FakeDailyReviewRepository([stale, reviewedToday, done]);
        repository.SeedHistory(stale, "created", Yesterday, new { collection = "today" });
        repository.SeedHistory(reviewedToday, "created", Yesterday, new { collection = "today" });
        repository.SeedHistory(reviewedToday, "daily_reviewed", Now, new { decision = "keep_today", collection = "today" });

        var useCase = new GetDailyReviewItemsUseCase(repository, repository, new FixedClock(Now));

        var result = await useCase.ExecuteAsync();

        var item = Assert.Single(result);
        Assert.Equal("t-0426-1", item.Item.PublicRef);
        Assert.Equal(DateOnly.FromDateTime(Yesterday.Date), item.LastTodayPlacementDate);
    }

    [Fact]
    public async Task GetDailyReviewItemsAsync_uses_latest_migration_to_today_as_trace_date()
    {
        var item = CreateTask("t-0426-1", "Migrated task", ItemStatus.Open);
        var repository = new FakeDailyReviewRepository([item]);
        repository.SeedHistory(item, "created", Now.AddDays(-5), new { collection = "backlog" });
        repository.SeedHistory(item, "migrate", Yesterday, new { from_collection = "backlog", to_collection = "today" });

        var useCase = new GetDailyReviewItemsUseCase(repository, repository, new FixedClock(Now));

        var result = await useCase.ExecuteAsync();

        var reviewItem = Assert.Single(result);
        Assert.Equal(DateOnly.FromDateTime(Yesterday.Date), reviewItem.LastTodayPlacementDate);
    }

    [Fact]
    public async Task KeepTodayAsync_appends_history_without_updating_item()
    {
        var item = CreateTask("t-0426-1", "Keep this", ItemStatus.Open, updatedAt: Yesterday);
        var repository = new FakeDailyReviewRepository([item]);
        var useCase = new KeepTodayItemUseCase(repository, repository);

        var result = await useCase.ExecuteAsync("t-0426-1");

        Assert.Equal("t-0426-1", result.PublicRef);
        Assert.Equal(0, repository.UpdateCalls);
        var history = Assert.Single(repository.AppendedHistory);
        Assert.Equal(item.Id, history.ItemId);
        Assert.Equal("daily_reviewed", history.EventType);
        Assert.Contains("keep_today", history.DataJson, StringComparison.Ordinal);
    }

    private static Item CreateTask(
        string publicRef,
        string content,
        ItemStatus status,
        DateTimeOffset? completedAt = null,
        DateTimeOffset? cancelledAt = null,
        DateTimeOffset? updatedAt = null)
    {
        return Item.Restore(
            Guid.NewGuid(),
            PublicRef.Parse(publicRef),
            ItemType.Task,
            content,
            null,
            status,
            ItemCollection.Today,
            Priority.None,
            Item.DefaultTag,
            1,
            Yesterday,
            updatedAt ?? Yesterday,
            completedAt: completedAt,
            cancelledAt: cancelledAt);
    }

    private sealed class FixedClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }

    private sealed class FakeDailyReviewRepository(IReadOnlyCollection<Item> items)
        : IItemRepository, IItemHistoryReader, IItemHistoryWriter
    {
        private readonly List<Item> _items = items.ToList();
        private readonly Dictionary<string, List<ItemHistoryEntry>> _historyByPublicRef = new(StringComparer.Ordinal);

        public int UpdateCalls { get; private set; }

        public List<ItemHistoryEntry> AppendedHistory { get; } = [];

        public void SeedHistory(Item item, string eventType, DateTimeOffset occurredAt, object data)
        {
            if (!_historyByPublicRef.TryGetValue(item.PublicRef.Value, out var entries))
            {
                entries = [];
                _historyByPublicRef[item.PublicRef.Value] = entries;
            }

            entries.Add(new ItemHistoryEntry(
                Guid.NewGuid(),
                item.Id,
                item.PublicRef.Value,
                eventType,
                occurredAt,
                JsonSerializer.Serialize(data)));
        }

        public Task AppendHistoryAsync(
            Guid itemId,
            string publicRef,
            string eventType,
            object? data = null,
            CancellationToken cancellationToken = default)
        {
            var entry = new ItemHistoryEntry(
                Guid.NewGuid(),
                itemId,
                publicRef,
                eventType,
                Now,
                JsonSerializer.Serialize(data ?? new { }));
            AppendedHistory.Add(entry);
            return Task.CompletedTask;
        }

        public Task<int> GetCurrentPublicRefSequenceAsync(ItemType type, int month, int year, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<bool> PublicRefExistsAsync(string publicRef, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task AddAsync(Item item, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task UpdateAsync(Item item, CancellationToken cancellationToken = default)
        {
            UpdateCalls++;
            return Task.CompletedTask;
        }

        public Task DeleteByPublicRefAsync(string publicRef, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task ClearHistoryAsync(CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<IReadOnlyCollection<Item>> ListAsync(
            ItemCollection? collection = null,
            ItemStatus? status = null,
            CancellationToken cancellationToken = default)
        {
            IEnumerable<Item> query = _items;
            if (collection is not null)
            {
                query = query.Where(item => item.Collection == collection.Value);
            }

            if (status is not null)
            {
                query = query.Where(item => item.Status == status.Value);
            }

            return Task.FromResult<IReadOnlyCollection<Item>>(query.ToArray());
        }

        public Task<Item?> FindByPublicRefAsync(string publicRef, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_items.FirstOrDefault(item => item.PublicRef.Value == publicRef));
        }

        public Task<IReadOnlyCollection<ItemHistoryEntry>> ListHistoryByPublicRefAsync(
            string publicRef,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<ItemHistoryEntry>>(
                _historyByPublicRef.TryGetValue(publicRef, out var entries)
                    ? entries
                    : []);
        }
    }
}
