using TermBullet.Application.Ports;
using TermBullet.Application.Tags;
using TermBullet.Core.Tags;

namespace TermBullet.Tests.Application.Tags;

public sealed class TagCatalogUseCaseTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 9, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task CreateTag_creates_normalized_tag()
    {
        var repository = new FakeTagCatalogRepository();
        var useCase = new CreateTagUseCase(repository, new FixedClock(Now));

        var result = await useCase.ExecuteAsync(new CreateTagRequest
        {
            Name = "  Auth  ",
            Description = "  Authentication work  "
        });

        Assert.Equal("auth", result.Name);
        Assert.Equal("Authentication work", result.Description);
        Assert.Equal(Now, result.CreatedAt);
        Assert.Single(repository.Tags);
    }

    [Fact]
    public async Task CreateTag_rejects_duplicate_normalized_name()
    {
        var repository = new FakeTagCatalogRepository();
        await repository.AddAsync(TagCatalogEntry.Create("auth", null, Now));
        var useCase = new CreateTagUseCase(repository, new FixedClock(Now));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(new CreateTagRequest { Name = "AUTH" }));
    }

    [Fact]
    public async Task ListTags_returns_catalog_sorted_by_name()
    {
        var repository = new FakeTagCatalogRepository();
        await repository.AddAsync(TagCatalogEntry.Create("tui", null, Now));
        await repository.AddAsync(TagCatalogEntry.Create("auth", null, Now));
        var useCase = new ListTagsUseCase(repository);

        var tags = await useCase.ExecuteAsync();

        Assert.Equal(["auth", "tui"], tags.Select(tag => tag.Name));
    }

    private sealed class FakeTagCatalogRepository : ITagCatalogRepository
    {
        public List<TagCatalogEntry> Tags { get; } = [];

        public Task<IReadOnlyCollection<TagCatalogEntry>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<TagCatalogEntry>>(Tags);

        public Task<TagCatalogEntry?> FindByNameAsync(string name, CancellationToken cancellationToken = default) =>
            Task.FromResult(Tags.FirstOrDefault(tag => string.Equals(tag.Name, name.Trim().ToLowerInvariant(), StringComparison.OrdinalIgnoreCase)));

        public Task AddAsync(TagCatalogEntry tag, CancellationToken cancellationToken = default)
        {
            Tags.Add(tag);
            return Task.CompletedTask;
        }
    }

    private sealed class FixedClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }
}
