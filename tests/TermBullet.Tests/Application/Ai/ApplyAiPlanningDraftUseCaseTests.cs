using TermBullet.Application.Ai;
using TermBullet.Application.Items;
using TermBullet.Application.Tags;
using TermBullet.Domain.Items;
using TermBullet.Domain.Refs;
using TermBullet.Domain.Tags;
using TermBullet.Repositories.Interfaces;
using TermBullet.Services.Clock;
using TermBullet.Services.Ids;

namespace TermBullet.Tests.Application.Ai;

public sealed class ApplyAiPlanningDraftUseCaseTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 2, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ExecuteAsync_applies_new_project_actions_in_order()
    {
        var itemRepository = new FakeItemRepository();
        var tagRepository = new FakeTagCatalogRepository();
        var useCase = CreateUseCase(itemRepository, tagRepository);
        var draft = AiPlanningDraftParser.Parse(
            """
            {
              "mode": "new_project",
              "summary": "Java study roadmap.",
              "actions": [
                { "type": "create_tag", "name": "estudo-java" },
                {
                  "type": "create_note",
                  "tag": "estudo-java",
                  "content": "Java study roadmap",
                  "description": "Scope and study sequence."
                },
                {
                  "type": "create_task",
                  "tag": "estudo-java",
                  "collection": "today",
                  "content": "Install JDK and run Hello World",
                  "priority": "high"
                },
                {
                  "type": "create_task",
                  "tag": "estudo-java",
                  "collection": "week",
                  "content": "Study Java syntax"
                }
              ]
            }
            """);

        var result = await useCase.ExecuteAsync(draft);

        Assert.Equal(["create_tag", "create_note", "create_task", "create_task"], result.Actions.Select(action => action.Type));
        Assert.Equal("estudo-java", Assert.Single(tagRepository.Tags).Name);
        Assert.Collection(
            itemRepository.Items,
            item =>
            {
                Assert.Equal("n-0626-1", item.PublicRef.Value);
                Assert.Equal(ItemType.Note, item.Type);
                Assert.Equal(ItemCollection.Notes, item.Collection);
                Assert.Equal("estudo-java", item.Tag);
            },
            item =>
            {
                Assert.Equal("t-0626-1", item.PublicRef.Value);
                Assert.Equal(ItemType.Task, item.Type);
                Assert.Equal(ItemCollection.Today, item.Collection);
                Assert.Equal(Priority.High, item.Priority);
                Assert.Equal("estudo-java", item.Tag);
            },
            item =>
            {
                Assert.Equal("t-0626-2", item.PublicRef.Value);
                Assert.Equal(ItemType.Task, item.Type);
                Assert.Equal(ItemCollection.Week, item.Collection);
                Assert.Equal(Priority.None, item.Priority);
                Assert.Equal("estudo-java", item.Tag);
            });
    }

    [Fact]
    public async Task ExecuteAsync_rejects_invalid_draft_before_persisting()
    {
        var itemRepository = new FakeItemRepository();
        var tagRepository = new FakeTagCatalogRepository();
        var useCase = CreateUseCase(itemRepository, tagRepository);
        var draft = AiPlanningDraftParser.Parse(
            """
            {
              "mode": "new_weekly",
              "summary": "Weekly plan.",
              "actions": [
                {
                  "type": "create_task",
                  "tag": "project",
                  "collection": "week",
                  "content": "This should not be applied"
                }
              ]
            }
            """);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(draft));

        Assert.Contains("invalid", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(itemRepository.Items);
        Assert.Empty(tagRepository.Tags);
    }

    private static ApplyAiPlanningDraftUseCase CreateUseCase(
        FakeItemRepository itemRepository,
        FakeTagCatalogRepository tagRepository)
    {
        var clock = new FixedClock(Now);
        var idGenerator = new IncrementingIdGenerator();

        return new ApplyAiPlanningDraftUseCase(
            new AiPlanningDraftValidator(),
            new CreateTagUseCase(tagRepository, clock),
            new CreateItemUseCase(itemRepository, clock, idGenerator));
    }

    private sealed class FakeItemRepository(IReadOnlyCollection<Item>? seed = null) : IItemRepository
    {
        private readonly List<Item> items = seed?.ToList() ?? [];

        public IReadOnlyList<Item> Items => items;

        public Task<int> GetCurrentPublicRefSequenceAsync(
            ItemType type,
            int month,
            int year,
            CancellationToken cancellationToken = default)
        {
            var prefix = type switch
            {
                ItemType.Task => "t",
                ItemType.Note => "n",
                ItemType.Event => "e",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported item type.")
            };
            var marker = $"{prefix}-{month:00}{year % 100:00}-";
            var sequence = items
                .Select(item => item.PublicRef.Value)
                .Where(value => value.StartsWith(marker, StringComparison.OrdinalIgnoreCase))
                .Select(value => int.Parse(value.Split('-')[^1]))
                .DefaultIfEmpty(0)
                .Max();

            return Task.FromResult(sequence);
        }

        public Task<bool> PublicRefExistsAsync(string publicRef, CancellationToken cancellationToken = default) =>
            Task.FromResult(items.Any(item => string.Equals(item.PublicRef.Value, publicRef, StringComparison.OrdinalIgnoreCase)));

        public Task AddAsync(Item item, CancellationToken cancellationToken = default)
        {
            items.Add(item);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Item item, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task DeleteByPublicRefAsync(string publicRef, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task ClearHistoryAsync(CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<IReadOnlyCollection<Item>> ListAsync(
            ItemCollection? collection = null,
            ItemStatus? status = null,
            CancellationToken cancellationToken = default)
        {
            var filtered = items.Where(item =>
                (collection is null || item.Collection == collection) &&
                (status is null || item.Status == status));

            return Task.FromResult<IReadOnlyCollection<Item>>(filtered.ToArray());
        }

        public Task<Item?> FindByPublicRefAsync(string publicRef, CancellationToken cancellationToken = default) =>
            Task.FromResult(items.FirstOrDefault(item =>
                string.Equals(item.PublicRef.Value, publicRef, StringComparison.OrdinalIgnoreCase)));
    }

    private sealed class FakeTagCatalogRepository : ITagCatalogRepository
    {
        private readonly List<TagCatalogEntry> tags = [];

        public IReadOnlyList<TagCatalogEntry> Tags => tags;

        public Task<IReadOnlyCollection<TagCatalogEntry>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<TagCatalogEntry>>(tags.ToArray());

        public Task<TagCatalogEntry?> FindByNameAsync(string name, CancellationToken cancellationToken = default) =>
            Task.FromResult(tags.FirstOrDefault(tag =>
                string.Equals(tag.Name, name.Trim(), StringComparison.OrdinalIgnoreCase)));

        public Task AddAsync(TagCatalogEntry tag, CancellationToken cancellationToken = default)
        {
            tags.Add(tag);
            return Task.CompletedTask;
        }
    }

    private sealed class FixedClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }

    private sealed class IncrementingIdGenerator : IIdGenerator
    {
        private int next = 1;

        public Guid NewId()
        {
            var bytes = new byte[16];
            BitConverter.GetBytes(next++).CopyTo(bytes, 0);
            return new Guid(bytes);
        }
    }
}
