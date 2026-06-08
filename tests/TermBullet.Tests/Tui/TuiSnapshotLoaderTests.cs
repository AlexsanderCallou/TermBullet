using TermBullet.Application.Items;
using System.Text.Json;
using TermBullet.Repositories.Interfaces;
using TermBullet.Application.Tags;
using TermBullet.Domain.Items;
using TermBullet.Domain.Tags;
using TermBullet.Services.Clock;
using TermBullet.Tui;

namespace TermBullet.Tests.Tui;

public sealed class TuiSnapshotLoaderTests
{
    private static readonly DateTimeOffset Now = new(2026, 4, 23, 10, 30, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Yesterday = Now.AddDays(-1);

    [Fact]
    public async Task LoadAsync_runs_startup_action_only_once_before_first_snapshot()
    {
        var repository = new FakeItemRepository();
        var tagRepository = new FakeTagCatalogRepository();
        var startupCalls = 0;
        var loader = CreateLoader(
            repository,
            tagRepository,
            _ =>
            {
                startupCalls++;
                return Task.CompletedTask;
            });

        await loader.LoadAsync();
        await loader.LoadAsync();

        Assert.Equal(1, startupCalls);
    }

    [Fact]
    public async Task LoadAsync_returns_today_backlog_and_tags()
    {
        var repository = new FakeItemRepository();
        var tagRepository = new FakeTagCatalogRepository();
        repository.Seed(MakeItem("t-0426-1", ItemCollection.Today, "Fix auth"));
        repository.Seed(MakeItem("t-0426-2", ItemCollection.Backlog, "Review migrations"));
        await tagRepository.AddAsync(TagCatalogEntry.Create("auth", null, DateTimeOffset.UtcNow));

        var loader = CreateLoader(repository, tagRepository);

        var snapshot = await loader.LoadAsync();

        Assert.Single(snapshot.TodayItems);
        Assert.Single(snapshot.BacklogItems);
        Assert.Single(snapshot.Tags);
    }

    [Fact]
    public async Task LoadAsync_keeps_current_items_separate_from_archive_items()
    {
        var repository = new FakeItemRepository();
        var tagRepository = new FakeTagCatalogRepository();
        repository.Seed(MakeItem("t-0526-1", ItemCollection.Today, "Current task"));
        repository.SeedArchive(MakeItem("t-0426-1", ItemCollection.Today, "Old forgotten task"));

        var loader = CreateLoader(repository, tagRepository);

        var snapshot = await loader.LoadAsync();

        Assert.Contains(snapshot.CurrentItems, item => item.PublicRef == "t-0526-1");
        Assert.DoesNotContain(snapshot.CurrentItems, item => item.PublicRef == "t-0426-1");
        Assert.Contains(snapshot.AllItems, item => item.PublicRef == "t-0426-1");
    }

    [Fact]
    public async Task LoadAsync_returns_daily_review_items()
    {
        var repository = new FakeItemRepository();
        var tagRepository = new FakeTagCatalogRepository();
        var item = MakeItem("t-0426-1", ItemCollection.Today, "Stale today task", Yesterday);
        repository.Seed(item);
        repository.SeedHistory(item, "created", Yesterday, new { collection = "today" });

        var loader = CreateLoader(repository, tagRepository);

        var snapshot = await loader.LoadAsync();

        var reviewItem = Assert.Single(snapshot.DailyReviewItems);
        Assert.Equal("t-0426-1", reviewItem.Item.PublicRef);
    }


    private static TuiSnapshotLoader CreateLoader(
        FakeItemRepository repository,
        FakeTagCatalogRepository tagRepository,
        Func<CancellationToken, Task>? startupAction = null)
    {
        return new TuiSnapshotLoader(
            new GetTodayItemsUseCase(repository, new FixedClock(DateTimeOffset.UtcNow)),
            new GetWeekItemsUseCase(repository),
            new GetMonthItemsUseCase(repository),
            new GetBacklogItemsUseCase(repository),
            new ListItemsUseCase(repository),
            new ListTagsUseCase(tagRepository),
            new GetDailyReviewItemsUseCase(repository, repository, new FixedClock(Now)),
            startupAction);
    }

    private static Item MakeItem(string publicRef, ItemCollection collection, string content) =>
        MakeItem(publicRef, collection, content, DateTimeOffset.UtcNow);

    private static Item MakeItem(string publicRef, ItemCollection collection, string content, DateTimeOffset createdAt) =>
        Item.Create(
            Guid.NewGuid(),
            TermBullet.Domain.Refs.PublicRef.Parse(publicRef),
            ItemType.Task,
            content,
            collection,
            createdAt);

    private sealed class FixedClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }

    private sealed class FakeItemRepository : IItemRepository, IItemArchiveReader, IItemHistoryReader
    {
        private readonly List<Item> _items = [];
        private readonly List<Item> _archiveItems = [];
        private readonly Dictionary<string, List<ItemHistoryEntry>> _historyByPublicRef = new(StringComparer.Ordinal);

        public void Seed(Item item) => _items.Add(item);

        public void SeedArchive(Item item) => _archiveItems.Add(item);

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

        public Task<int> GetCurrentPublicRefSequenceAsync(ItemType type, int month, int year, CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public Task<bool> PublicRefExistsAsync(string publicRef, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.Any(item => item.PublicRef.Value == publicRef));

        public Task AddAsync(Item item, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task UpdateAsync(Item item, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task DeleteByPublicRefAsync(string publicRef, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task ClearHistoryAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<IReadOnlyCollection<Item>> ListAsync(ItemCollection? collection = null, ItemStatus? status = null, CancellationToken cancellationToken = default)
        {
            var query = _items.AsEnumerable();

            if (collection is not null)
            {
                query = query.Where(item => item.Collection == collection);
            }

            if (status is not null)
            {
                query = query.Where(item => item.Status == status);
            }

            return Task.FromResult<IReadOnlyCollection<Item>>(query.ToArray());
        }

        public Task<Item?> FindByPublicRefAsync(string publicRef, CancellationToken cancellationToken = default)
            => Task.FromResult<Item?>(_items.FirstOrDefault(item => item.PublicRef.Value == publicRef));

        public Task<IReadOnlyCollection<Item>> ListAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Item>>(_items.Concat(_archiveItems).ToArray());

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

    private sealed class FakeTagCatalogRepository : ITagCatalogRepository
    {
        private readonly List<TagCatalogEntry> _tags = [];

        public Task<IReadOnlyCollection<TagCatalogEntry>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<TagCatalogEntry>>(_tags);

        public Task<TagCatalogEntry?> FindByNameAsync(string name, CancellationToken cancellationToken = default) =>
            Task.FromResult(_tags.FirstOrDefault(tag => string.Equals(tag.Name, name.Trim(), StringComparison.OrdinalIgnoreCase)));

        public Task AddAsync(TagCatalogEntry tag, CancellationToken cancellationToken = default)
        {
            _tags.Add(tag);
            return Task.CompletedTask;
        }
    }
}
