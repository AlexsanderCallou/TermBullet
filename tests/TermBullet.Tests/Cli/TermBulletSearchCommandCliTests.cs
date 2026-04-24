using System.Text;
using TermBullet.Application.Configuration;
using TermBullet.Application.DataTransfer;
using TermBullet.Application.History;
using TermBullet.Application.Items;
using TermBullet.Application.Ports;
using TermBullet.Cli;
using TermBullet.Core.Items;

namespace TermBullet.Tests.Cli;

public sealed class TermBulletSearchCommandCliTests
{
    private static readonly DateTimeOffset Now = new(2026, 4, 23, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task InvokeAsync_runs_search_and_writes_matching_items()
    {
        var repository = new FakeItemRepository();
        repository.Seed(CreateItemResult("t-0426-1", "Fix auth flow", "auth"));
        repository.Seed(CreateItemResult("t-0426-2", "Review notes", "docs"));
        var app = CreateApp(repository);

        var exitCode = await app.App.InvokeAsync(["search", "auth"]);

        Assert.Equal(0, exitCode);
        Assert.Contains("t-0426-1", app.Output.ToString());
        Assert.Contains("Fix auth flow", app.Output.ToString());
        Assert.DoesNotContain("t-0426-2", app.Output.ToString());
    }

    [Fact]
    public async Task InvokeAsync_returns_error_for_empty_search_query()
    {
        var repository = new FakeItemRepository();
        var app = CreateApp(repository);

        var exitCode = await app.App.InvokeAsync(["search", " "]);

        Assert.Equal(1, exitCode);
        Assert.Contains("Query must not be empty", app.Error.ToString());
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
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                new SearchItemsUseCase(repository)),
            output,
            error);
    }

    private static ItemResult CreateItemResult(string publicRef, string content, string tag)
    {
        return new ItemResult(
            Guid.NewGuid(),
            publicRef,
            ItemType.Task,
            content,
            null,
            ItemStatus.Open,
            ItemCollection.Today,
            Priority.None,
            [tag],
            1,
            Now,
            Now);
    }

    private sealed record TestCliApp(TermBulletCliApp App, StringWriter Output, StringWriter Error);

    private sealed class FakeItemRepository : IItemRepository
    {
        private readonly List<Item> _items = [];

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
            => Task.FromResult(0);

        public Task<bool> PublicRefExistsAsync(string publicRef, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task AddAsync(Item item, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task UpdateAsync(Item item, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task DeleteByPublicRefAsync(string publicRef, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task ClearHistoryAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<IReadOnlyCollection<Item>> ListAsync(ItemCollection? collection = null, ItemStatus? status = null, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Item>>(_items.ToArray());

        public Task<Item?> FindByPublicRefAsync(string publicRef, CancellationToken cancellationToken = default)
            => Task.FromResult<Item?>(null);
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
}
