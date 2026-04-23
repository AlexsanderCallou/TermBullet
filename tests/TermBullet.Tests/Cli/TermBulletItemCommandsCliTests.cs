using System.Text;
using TermBullet.Application.Configuration;
using TermBullet.Application.DataTransfer;
using TermBullet.Application.History;
using TermBullet.Application.Items;
using TermBullet.Application.Ports;
using TermBullet.Cli;
using TermBullet.Core.Items;

namespace TermBullet.Tests.Cli;

public sealed class TermBulletItemCommandsCliTests
{
    private static readonly DateTimeOffset Now = new(2026, 4, 23, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task InvokeAsync_runs_add_and_creates_task_with_public_ref()
    {
        var repository = new FakeItemRepository();
        var testApp = CreateApp(repository);

        var exitCode = await testApp.App.InvokeAsync(["add", "Fix authentication flow"]);

        Assert.Equal(0, exitCode);
        var item = Assert.Single(repository.Items);
        Assert.Equal(ItemType.Task, item.Type);
        Assert.Equal("t-0426-1", item.PublicRef.Value);
        Assert.Contains("t-0426-1", testApp.Output.ToString());
        Assert.Contains("Fix authentication flow", testApp.Output.ToString());
    }

    [Fact]
    public async Task InvokeAsync_runs_add_with_note_flag()
    {
        var repository = new FakeItemRepository();
        var testApp = CreateApp(repository);

        var exitCode = await testApp.App.InvokeAsync(["add", "Investigate stacktrace", "--note"]);

        Assert.Equal(0, exitCode);
        var item = Assert.Single(repository.Items);
        Assert.Equal(ItemType.Note, item.Type);
        Assert.Equal("n-0426-1", item.PublicRef.Value);
    }

    [Fact]
    public async Task InvokeAsync_runs_list_and_writes_matching_items()
    {
        var repository = new FakeItemRepository();
        var item = CreateItemResult("t-0426-1", ItemType.Task, ItemCollection.Today, ItemStatus.Open, "Fix auth");
        repository.Seed(item);
        var testApp = CreateApp(repository);

        var exitCode = await testApp.App.InvokeAsync(["list", "--collection", "today", "--status", "open"]);

        Assert.Equal(0, exitCode);
        Assert.Contains("t-0426-1", testApp.Output.ToString());
        Assert.Contains("Fix auth", testApp.Output.ToString());
    }

    [Fact]
    public async Task InvokeAsync_runs_show_and_writes_item_details()
    {
        var repository = new FakeItemRepository();
        var item = CreateItemResult("t-0426-1", ItemType.Task, ItemCollection.Today, ItemStatus.Open, "Fix auth");
        repository.Seed(item);
        var testApp = CreateApp(repository);

        var exitCode = await testApp.App.InvokeAsync(["show", "t-0426-1"]);

        Assert.Equal(0, exitCode);
        Assert.Contains("t-0426-1", testApp.Output.ToString());
        Assert.Contains("Fix auth", testApp.Output.ToString());
        Assert.Contains("today", testApp.Output.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InvokeAsync_runs_today_command()
    {
        var repository = new FakeItemRepository();
        repository.Seed(CreateItemResult("t-0426-1", ItemType.Task, ItemCollection.Today, ItemStatus.Open, "Today item"));
        repository.Seed(CreateItemResult("t-0426-2", ItemType.Task, ItemCollection.Backlog, ItemStatus.Open, "Backlog item"));
        var testApp = CreateApp(repository);

        var exitCode = await testApp.App.InvokeAsync(["today"]);

        Assert.Equal(0, exitCode);
        Assert.Contains("Today item", testApp.Output.ToString());
        Assert.DoesNotContain("Backlog item", testApp.Output.ToString());
    }

    [Fact]
    public async Task InvokeAsync_runs_week_and_backlog_commands()
    {
        var repository = new FakeItemRepository();
        repository.Seed(CreateItemResult("t-0426-1", ItemType.Task, ItemCollection.Week, ItemStatus.Open, "Week item"));
        repository.Seed(CreateItemResult("t-0426-2", ItemType.Task, ItemCollection.Backlog, ItemStatus.Open, "Backlog item"));
        var weekApp = CreateApp(repository);
        var backlogApp = CreateApp(repository);

        var weekExitCode = await weekApp.App.InvokeAsync(["week"]);
        var backlogExitCode = await backlogApp.App.InvokeAsync(["backlog"]);

        Assert.Equal(0, weekExitCode);
        Assert.Equal(0, backlogExitCode);
        Assert.Contains("Week item", weekApp.Output.ToString());
        Assert.DoesNotContain("Backlog item", weekApp.Output.ToString());
        Assert.Contains("Backlog item", backlogApp.Output.ToString());
    }

    private static TestCliApp CreateApp(FakeItemRepository repository)
    {
        var settingsStore = new FakeSettingsStore();
        var dataTransferService = new FakeDataTransferService();
        var historyService = new FakeHistoryMaintenanceService();
        var output = new StringWriter(new StringBuilder());
        var error = new StringWriter(new StringBuilder());

        return new TestCliApp(
            new TermBulletCliApp(
            new ListConfigurationUseCase(settingsStore),
            new GetConfigurationUseCase(settingsStore),
            new SetConfigurationUseCase(settingsStore),
            new GetConfigurationPathUseCase(settingsStore),
            new ExportDataUseCase(dataTransferService),
            new ImportDataUseCase(dataTransferService),
            new ClearStoredHistoryUseCase(historyService, new FixedClock(Now)),
            output,
            error,
            new CreateItemUseCase(repository, new FixedClock(Now), new FixedIdGenerator()),
            new ListItemsUseCase(repository),
            new ShowItemUseCase(repository),
            new GetTodayItemsUseCase(repository),
            new GetWeekItemsUseCase(repository),
            new GetBacklogItemsUseCase(repository)),
            output,
            error);
    }

    private static ItemResult CreateItemResult(
        string publicRef,
        ItemType type,
        ItemCollection collection,
        ItemStatus status,
        string content)
    {
        return new ItemResult(
            Guid.NewGuid(),
            publicRef,
            type,
            content,
            null,
            status,
            collection,
            Priority.None,
            [],
            1,
            Now,
            Now);
    }

    private sealed record TestCliApp(TermBulletCliApp App, StringWriter Output, StringWriter Error);

    private sealed class FakeItemRepository : IItemRepository
    {
        private readonly List<Item> _items = [];

        public IReadOnlyCollection<Item> Items => _items;

        public void Seed(ItemResult itemResult)
        {
            _items.Add(Item.Restore(
                itemResult.Id,
                TermBullet.Core.Refs.PublicRef.Parse(itemResult.PublicRef),
                itemResult.Type,
                itemResult.Content,
                itemResult.Description,
                itemResult.Status,
                itemResult.Collection,
                itemResult.Priority,
                itemResult.Tags,
                itemResult.Version,
                itemResult.CreatedAt,
                itemResult.UpdatedAt));
        }

        public Task<int> GetCurrentPublicRefSequenceAsync(ItemType type, int month, int year, CancellationToken cancellationToken = default)
        {
            var max = _items
                .Where(item => item.PublicRef.Type == type && item.PublicRef.Month == month && item.PublicRef.YearTwoDigits == year % 100)
                .Select(item => item.PublicRef.Sequence)
                .DefaultIfEmpty(0)
                .Max();
            return Task.FromResult(max);
        }

        public Task<bool> PublicRefExistsAsync(string publicRef, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.Any(item => string.Equals(item.PublicRef.Value, publicRef, StringComparison.Ordinal)));

        public Task AddAsync(Item item, CancellationToken cancellationToken = default)
        {
            _items.Add(item);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Item item, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task DeleteByPublicRefAsync(string publicRef, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task ClearHistoryAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<IReadOnlyCollection<Item>> ListAsync(ItemCollection? collection = null, ItemStatus? status = null, CancellationToken cancellationToken = default)
        {
            IEnumerable<Item> items = _items;
            if (collection is not null)
            {
                items = items.Where(item => item.Collection == collection.Value);
            }

            if (status is not null)
            {
                items = items.Where(item => item.Status == status.Value);
            }

            return Task.FromResult<IReadOnlyCollection<Item>>(items.ToArray());
        }

        public Task<Item?> FindByPublicRefAsync(string publicRef, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.FirstOrDefault(item => string.Equals(item.PublicRef.Value, publicRef, StringComparison.Ordinal)));
    }

    private sealed class FakeSettingsStore : ISettingsStore
    {
        public string SettingsPath => "C:\\term\\data\\settings.json";

        public Task<string?> GetAsync(string key, string profile = "default", CancellationToken cancellationToken = default)
            => Task.FromResult<string?>(null);

        public Task<IReadOnlyDictionary<string, string>> ListAsync(string profile = "default", CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyDictionary<string, string>>(new Dictionary<string, string>());

        public Task SetAsync(string key, string value, string profile = "default", CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeDataTransferService : IDataTransferService
    {
        public Task ExportAsync(string outputPath, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task ImportAsync(string inputPath, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeHistoryMaintenanceService : IHistoryMaintenanceService
    {
        public Task ClearMonthAsync(int month, int year, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task ClearAllAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FixedClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }

    private sealed class FixedIdGenerator : IIdGenerator
    {
        private int _counter;

        public Guid NewId()
        {
            _counter++;
            return _counter switch
            {
                1 => Guid.Parse("0f3a9d94-4df0-47f7-95c1-0f967c22f4db"),
                2 => Guid.Parse("c4dbec0e-c42d-4f26-8659-05bfdb4db056"),
                _ => Guid.NewGuid()
            };
        }
    }
}
