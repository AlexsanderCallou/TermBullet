using System.Text;
using System.Text.Json;
using TermBullet.Application.History;
using TermBullet.Application.Items;
using TermBullet.Cli;
using TermBullet.Domain.Items;
using TermBullet.Domain.Refs;
using TermBullet.Repositories.Interfaces;
using TermBullet.Services.Clock;
using TermBullet.Services.History;

namespace TermBullet.Tests.Cli;

public sealed class TermBulletDailyReviewCliTests
{
    private static readonly DateTimeOffset Now = new(2026, 4, 23, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Yesterday = Now.AddDays(-1);

    [Fact]
    public async Task InvokeAsync_runs_daily_review_and_lists_stale_today_tasks()
    {
        var repository = CreateSeededRepository();
        var app = CreateApp(repository);

        var exitCode = await app.App.InvokeAsync(["daily", "review"]);

        Assert.Equal(0, exitCode);
        Assert.Contains("t-0426-1", app.Output.ToString());
        Assert.Contains("last:2026-04-22", app.Output.ToString());
    }

    [Fact]
    public async Task InvokeAsync_runs_daily_keep_and_records_review_history_only()
    {
        var repository = CreateSeededRepository();
        var before = repository.Items.Single();
        var app = CreateApp(repository);

        var exitCode = await app.App.InvokeAsync(["daily", "keep", "t-0426-1"]);

        Assert.Equal(0, exitCode);
        var after = repository.Items.Single();
        Assert.Equal(before.UpdatedAt, after.UpdatedAt);
        Assert.Equal(before.Version, after.Version);
        Assert.Contains(repository.History, entry => entry.EventType == "daily_reviewed");
    }

    [Fact]
    public async Task InvokeAsync_runs_daily_move_done_and_cancel_actions()
    {
        var moveRepository = CreateSeededRepository();
        var moveApp = CreateApp(moveRepository);
        var moveExitCode = await moveApp.App.InvokeAsync(["daily", "move", "t-0426-1", "--collection", "week"]);

        var doneRepository = CreateSeededRepository();
        var doneApp = CreateApp(doneRepository);
        var doneExitCode = await doneApp.App.InvokeAsync(["daily", "done", "t-0426-1"]);

        var cancelRepository = CreateSeededRepository();
        var cancelApp = CreateApp(cancelRepository);
        var cancelExitCode = await cancelApp.App.InvokeAsync(["daily", "cancel", "t-0426-1"]);

        Assert.Equal(0, moveExitCode);
        Assert.Equal(ItemCollection.Week, moveRepository.Items.Single().Collection);
        Assert.Equal(0, doneExitCode);
        Assert.Equal(ItemStatus.Done, doneRepository.Items.Single().Status);
        Assert.Equal(0, cancelExitCode);
        Assert.Equal(ItemStatus.Cancelled, cancelRepository.Items.Single().Status);
    }

    private static TestCliApp CreateApp(FakeDailyReviewRepository repository)
    {
        var clock = new FixedClock(Now);
        var output = new StringWriter(new StringBuilder());
        var error = new StringWriter(new StringBuilder());
        var historyService = new FakeHistoryMaintenanceService();

        return new TestCliApp(
            new TermBulletCliApp(
                new ClearStoredHistoryUseCase(historyService, clock),
                output,
                error,
                markDoneItemUseCase: new MarkDoneItemUseCase(repository, clock),
                cancelItemUseCase: new CancelItemUseCase(repository, clock),
                migrateItemUseCase: new MigrateItemUseCase(repository, clock),
                startupAction: null,
                getDailyReviewItemsUseCase: new GetDailyReviewItemsUseCase(repository, repository, clock),
                keepTodayItemUseCase: new KeepTodayItemUseCase(repository, repository)),
            output,
            error);
    }

    private static FakeDailyReviewRepository CreateSeededRepository()
    {
        var repository = new FakeDailyReviewRepository();
        var item = Item.Create(
            Guid.Parse("0f3a9d94-4df0-47f7-95c1-0f967c22f4db"),
            PublicRef.Parse("t-0426-1"),
            ItemType.Task,
            "Review stale task",
            ItemCollection.Today,
            Yesterday);
        repository.Items.Add(item);
        repository.SeedHistory(item, "created", Yesterday, new { collection = "today" });
        return repository;
    }

    private sealed record TestCliApp(TermBulletCliApp App, StringWriter Output, StringWriter Error);

    private sealed class FakeDailyReviewRepository : IItemRepository, IItemHistoryReader, IItemHistoryWriter
    {
        private readonly Dictionary<string, List<ItemHistoryEntry>> _historyByPublicRef = new(StringComparer.Ordinal);

        public List<Item> Items { get; } = [];

        public List<ItemHistoryEntry> History { get; } = [];

        public void SeedHistory(Item item, string eventType, DateTimeOffset occurredAt, object data)
        {
            var entry = new ItemHistoryEntry(
                Guid.NewGuid(),
                item.Id,
                item.PublicRef.Value,
                eventType,
                occurredAt,
                JsonSerializer.Serialize(data));
            History.Add(entry);
            if (!_historyByPublicRef.TryGetValue(item.PublicRef.Value, out var entries))
            {
                entries = [];
                _historyByPublicRef[item.PublicRef.Value] = entries;
            }

            entries.Add(entry);
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
            History.Add(entry);
            if (!_historyByPublicRef.TryGetValue(publicRef, out var entries))
            {
                entries = [];
                _historyByPublicRef[publicRef] = entries;
            }

            entries.Add(entry);
            return Task.CompletedTask;
        }

        public Task<int> GetCurrentPublicRefSequenceAsync(ItemType type, int month, int year, CancellationToken cancellationToken = default)
            => Task.FromResult(1);

        public Task<bool> PublicRefExistsAsync(string publicRef, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.Any(item => item.PublicRef.Value == publicRef));

        public Task AddAsync(Item item, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task UpdateAsync(Item item, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task DeleteByPublicRefAsync(string publicRef, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task ClearHistoryAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<IReadOnlyCollection<Item>> ListAsync(
            ItemCollection? collection = null,
            ItemStatus? status = null,
            CancellationToken cancellationToken = default)
        {
            IEnumerable<Item> query = Items;
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
            => Task.FromResult<Item?>(Items.FirstOrDefault(item => item.PublicRef.Value == publicRef));

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

    private sealed class FixedClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }

    private sealed class FakeHistoryMaintenanceService : IHistoryMaintenanceService
    {
        public Task ClearMonthAsync(int month, int year, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task ClearAllAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
