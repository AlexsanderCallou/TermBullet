using TermBullet.Services.Clock;
using TermBullet.Services.History;
using System.Text;
using TermBullet.Application.History;
using TermBullet.Application.Items;
using TermBullet.Repositories.Interfaces;
using TermBullet.Cli;
using TermBullet.Domain.Items;

namespace TermBullet.Tests.Cli;

public sealed class TermBulletMutationCommandsCliTests
{
    private static readonly DateTimeOffset Now = new(2026, 4, 23, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task InvokeAsync_runs_edit_and_updates_item_content()
    {
        var repository = CreateSeededRepository();
        var app = CreateApp(repository);

        var exitCode = await app.App.InvokeAsync(["edit", "t-0426-1", "Refactor auth flow"]);

        Assert.Equal(0, exitCode);
        var item = Assert.Single(repository.Items);
        Assert.Equal("Refactor auth flow", item.Content);
        Assert.Contains("Refactor auth flow", app.Output.ToString());
    }

    [Fact]
    public async Task InvokeAsync_runs_done_and_updates_status()
    {
        var repository = CreateSeededRepository();
        var app = CreateApp(repository);

        var exitCode = await app.App.InvokeAsync(["done", "t-0426-1"]);

        Assert.Equal(0, exitCode);
        var item = Assert.Single(repository.Items);
        Assert.Equal(ItemStatus.Done, item.Status);
        Assert.Contains("done", app.Output.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InvokeAsync_runs_cancel_and_updates_status()
    {
        var repository = CreateSeededRepository();
        var app = CreateApp(repository);

        var exitCode = await app.App.InvokeAsync(["cancel", "t-0426-1"]);

        Assert.Equal(0, exitCode);
        var item = Assert.Single(repository.Items);
        Assert.Equal(ItemStatus.Cancelled, item.Status);
    }

    [Fact]
    public async Task InvokeAsync_runs_move_and_updates_collection()
    {
        var repository = CreateSeededRepository();
        var app = CreateApp(repository);

        var exitCode = await app.App.InvokeAsync(["move", "t-0426-1", "--to", "backlog"]);

        Assert.Equal(0, exitCode);
        var item = Assert.Single(repository.Items);
        Assert.Equal(ItemCollection.Backlog, item.Collection);
    }

    [Fact]
    public async Task InvokeAsync_runs_priority_and_updates_priority()
    {
        var repository = CreateSeededRepository();
        var app = CreateApp(repository);

        var exitCode = await app.App.InvokeAsync(["priority", "t-0426-1", "high"]);

        Assert.Equal(0, exitCode);
        var item = Assert.Single(repository.Items);
        Assert.Equal(Priority.High, item.Priority);
    }

    [Fact]
    public async Task InvokeAsync_runs_tag_and_untag()
    {
        var repository = CreateSeededRepository();
        var tagApp = CreateApp(repository);
        var untagApp = CreateApp(repository);

        var tagExitCode = await tagApp.App.InvokeAsync(["tag", "t-0426-1", "backend"]);
        var untagExitCode = await untagApp.App.InvokeAsync(["untag", "t-0426-1", "backend"]);

        Assert.Equal(0, tagExitCode);
        Assert.Equal(0, untagExitCode);
        var item = Assert.Single(repository.Items);
        Assert.DoesNotContain("backend", item.Tags);
    }

    [Fact]
    public async Task InvokeAsync_runs_migrate_to_week_and_moves_same_task()
    {
        var repository = CreateSeededRepository();
        var app = CreateApp(repository);

        var exitCode = await app.App.InvokeAsync(["migrate", "t-0426-1", "--collection", "week"]);

        Assert.Equal(0, exitCode);
        var item = Assert.Single(repository.Items);
        Assert.Equal("t-0426-1", item.PublicRef.Value);
        Assert.Equal(ItemStatus.Open, item.Status);
        Assert.Equal(ItemCollection.Week, item.Collection);
    }

    [Fact]
    public async Task InvokeAsync_runs_migrate_to_backlog_and_moves_same_task()
    {
        var repository = CreateSeededRepository();
        var app = CreateApp(repository);

        var exitCode = await app.App.InvokeAsync(["migrate", "t-0426-1", "--collection", "backlog"]);

        Assert.Equal(0, exitCode);
        var item = Assert.Single(repository.Items);
        Assert.Equal("t-0426-1", item.PublicRef.Value);
        Assert.Equal(ItemStatus.Open, item.Status);
        Assert.Equal(ItemCollection.Backlog, item.Collection);
    }

    [Fact]
    public async Task InvokeAsync_returns_error_when_migrate_destination_is_missing()
    {
        var repository = CreateSeededRepository();
        var app = CreateApp(repository);

        var exitCode = await app.App.InvokeAsync(["migrate", "t-0426-1"]);

        Assert.Equal(1, exitCode);
        Assert.Contains("--collection <today|week|month|backlog>", app.Error.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InvokeAsync_runs_delete_and_removes_item()
    {
        var repository = CreateSeededRepository();
        var app = CreateApp(repository);

        var exitCode = await app.App.InvokeAsync(["delete", "t-0426-1", "--force"]);

        Assert.Equal(0, exitCode);
        Assert.Empty(repository.Items);
        Assert.Contains("deleted: t-0426-1", app.Output.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InvokeAsync_returns_error_for_invalid_collection_value()
    {
        var repository = CreateSeededRepository();
        var app = CreateApp(repository);

        var exitCode = await app.App.InvokeAsync(["move", "t-0426-1", "--to", "invalid"]);

        Assert.Equal(1, exitCode);
        Assert.Contains("Unsupported collection", app.Error.ToString());
    }

    private static TestCliApp CreateApp(FakeItemRepository repository)
    {
        var historyService = new FakeHistoryMaintenanceService();
        var output = new StringWriter(new StringBuilder());
        var error = new StringWriter(new StringBuilder());
        var clock = new FixedClock(Now);

        return new TestCliApp(
            new TermBulletCliApp(
                new ClearStoredHistoryUseCase(historyService, clock),
                output,
                error,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                new EditItemUseCase(repository, clock),
                new MarkDoneItemUseCase(repository, clock),
                new CancelItemUseCase(repository, clock),
                new MoveItemUseCase(repository, clock),
                new SetItemPriorityUseCase(repository, clock),
                new TagItemUseCase(repository, clock),
                new UntagItemUseCase(repository, clock),
                new MigrateItemUseCase(repository, clock),
                new DeleteItemUseCase(repository)),
            output,
            error);
    }

    private static FakeItemRepository CreateSeededRepository()
    {
        var repository = new FakeItemRepository();
        repository.Items.Add(Item.Create(
            Guid.Parse("0f3a9d94-4df0-47f7-95c1-0f967c22f4db"),
            TermBullet.Domain.Refs.PublicRef.Parse("t-0426-1"),
            ItemType.Task,
            "Fix authentication flow",
            ItemCollection.Today,
            Now));
        return repository;
    }

    private sealed record TestCliApp(TermBulletCliApp App, StringWriter Output, StringWriter Error);

    private sealed class FakeItemRepository : IItemRepository
    {
        public List<Item> Items { get; } = [];

        public Task<int> GetCurrentPublicRefSequenceAsync(ItemType type, int month, int year, CancellationToken cancellationToken = default)
            => Task.FromResult(Items
                .Where(item => item.Type == type && item.CreatedAt.Month == month && item.CreatedAt.Year == year)
                .Select(item => item.PublicRef.Sequence)
                .DefaultIfEmpty(0)
                .Max());

        public Task<bool> PublicRefExistsAsync(string publicRef, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.Any(item => string.Equals(item.PublicRef.Value, publicRef, StringComparison.Ordinal)));

        public Task AddAsync(Item item, CancellationToken cancellationToken = default)
        {
            Items.Add(item);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Item item, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task DeleteByPublicRefAsync(string publicRef, CancellationToken cancellationToken = default)
        {
            var item = Items.FirstOrDefault(existingItem =>
                string.Equals(existingItem.PublicRef.Value, publicRef, StringComparison.Ordinal));

            if (item is null)
            {
                throw new KeyNotFoundException();
            }

            Items.Remove(item);
            return Task.CompletedTask;
        }

        public Task ClearHistoryAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<IReadOnlyCollection<Item>> ListAsync(ItemCollection? collection = null, ItemStatus? status = null, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Item>>(Items.ToArray());

        public Task<Item?> FindByPublicRefAsync(string publicRef, CancellationToken cancellationToken = default)
            => Task.FromResult<Item?>(Items.FirstOrDefault(item => string.Equals(item.PublicRef.Value, publicRef, StringComparison.Ordinal)));
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
