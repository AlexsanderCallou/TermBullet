using TermBullet.Application.Configuration;
using TermBullet.Application.Items;
using TermBullet.Application.Ports;
using TermBullet.Application.Tags;
using TermBullet.Core.Items;
using TermBullet.Core.Tags;
using TermBullet.Tui;

namespace TermBullet.Tests.Tui;

public sealed class TuiSnapshotLoaderTests
{
    [Fact]
    public async Task LoadAsync_runs_startup_action_only_once_before_first_snapshot()
    {
        var repository = new FakeItemRepository();
        var tagRepository = new FakeTagCatalogRepository();
        var settingsStore = new FakeSettingsStore();
        var startupCalls = 0;
        var loader = CreateLoader(
            repository,
            tagRepository,
            settingsStore,
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
    public async Task LoadAsync_returns_today_backlog_and_config_state()
    {
        var repository = new FakeItemRepository();
        var tagRepository = new FakeTagCatalogRepository();
        repository.Seed(MakeItem("t-0426-1", ItemCollection.Today, "Fix auth"));
        repository.Seed(MakeItem("t-0426-2", ItemCollection.Backlog, "Review migrations"));
        await tagRepository.AddAsync(TagCatalogEntry.Create("auth", null, DateTimeOffset.UtcNow));

        var settingsStore = new FakeSettingsStore();
        await settingsStore.SetAsync("theme", "dark");

        var loader = CreateLoader(repository, tagRepository, settingsStore);

        var snapshot = await loader.LoadAsync();

        Assert.Single(snapshot.TodayItems);
        Assert.Single(snapshot.BacklogItems);
        Assert.Single(snapshot.Tags);
        Assert.Equal("dark", snapshot.Configuration["theme"]);
    }

    [Fact]
    public async Task LoadAsync_keeps_current_items_separate_from_archive_items()
    {
        var repository = new FakeItemRepository();
        var tagRepository = new FakeTagCatalogRepository();
        var settingsStore = new FakeSettingsStore();
        repository.Seed(MakeItem("t-0526-1", ItemCollection.Today, "Current task"));
        repository.SeedArchive(MakeItem("t-0426-1", ItemCollection.Today, "Old forgotten task"));

        var loader = CreateLoader(repository, tagRepository, settingsStore);

        var snapshot = await loader.LoadAsync();

        Assert.Contains(snapshot.CurrentItems, item => item.PublicRef == "t-0526-1");
        Assert.DoesNotContain(snapshot.CurrentItems, item => item.PublicRef == "t-0426-1");
        Assert.Contains(snapshot.AllItems, item => item.PublicRef == "t-0426-1");
    }


    private static TuiSnapshotLoader CreateLoader(
        FakeItemRepository repository,
        FakeTagCatalogRepository tagRepository,
        FakeSettingsStore settingsStore,
        Func<CancellationToken, Task>? startupAction = null)
    {
        return new TuiSnapshotLoader(
            new GetTodayItemsUseCase(repository),
            new GetWeekItemsUseCase(repository),
            new GetBacklogItemsUseCase(repository),
            new ListItemsUseCase(repository),
            new ListTagsUseCase(tagRepository),
            new ListConfigurationUseCase(settingsStore),
            startupAction);
    }

    private static Item MakeItem(string publicRef, ItemCollection collection, string content) =>
        Item.Create(
            Guid.NewGuid(),
            TermBullet.Core.Refs.PublicRef.Parse(publicRef),
            ItemType.Task,
            content,
            collection,
            DateTimeOffset.UtcNow);

    private sealed class FakeItemRepository : IItemRepository, IItemArchiveReader
    {
        private readonly List<Item> _items = [];
        private readonly List<Item> _archiveItems = [];

        public void Seed(Item item) => _items.Add(item);

        public void SeedArchive(Item item) => _archiveItems.Add(item);

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
    }

    private sealed class FakeSettingsStore : ISettingsStore
    {
        private readonly Dictionary<string, string> _values = new(StringComparer.OrdinalIgnoreCase);

        public string SettingsPath => "settings.json";

        public Task<string?> GetAsync(string key, string profile = "default", CancellationToken cancellationToken = default)
            => Task.FromResult<string?>(_values.TryGetValue(key, out var value) ? value : null);

        public Task<IReadOnlyDictionary<string, string>> ListAsync(string profile = "default", CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyDictionary<string, string>>(new Dictionary<string, string>(_values, StringComparer.OrdinalIgnoreCase));

        public Task SetAsync(string key, string value, string profile = "default", CancellationToken cancellationToken = default)
        {
            _values[key] = value;
            return Task.CompletedTask;
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
