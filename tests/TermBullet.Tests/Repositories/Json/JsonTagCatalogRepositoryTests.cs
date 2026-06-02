using System.Text.Json;
using TermBullet.Domain.Tags;
using TermBullet.Repositories.Json;

namespace TermBullet.Tests.Repositories.Json;

public sealed class JsonTagCatalogRepositoryTests
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 5, 9, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task AddAsync_persists_tag_catalog_entry()
    {
        var context = CreateContext();
        var repository = CreateRepository(context);
        var tag = TagCatalogEntry.Create("Auth", "Authentication work", CreatedAt);

        await repository.AddAsync(tag);
        var tags = await repository.ListAsync();

        var stored = Assert.Single(tags, existing => existing.Name == "auth");
        Assert.Equal("auth", stored.Name);
        Assert.Equal("Authentication work", stored.Description);
        Assert.True(File.Exists(context.TagsPath));

        var json = await File.ReadAllTextAsync(context.TagsPath);
        using var doc = JsonDocument.Parse(json);
        var jsonTag = Assert.Single(
            doc.RootElement.GetProperty("tags").EnumerateArray(),
            existing => existing.GetProperty("name").GetString() == "auth");
        Assert.Equal("auth", jsonTag.GetProperty("name").GetString());
    }

    [Fact]
    public async Task AddAsync_rejects_duplicate_names_case_insensitively()
    {
        var repository = CreateRepository(CreateContext());

        await repository.AddAsync(TagCatalogEntry.Create("auth", null, CreatedAt));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repository.AddAsync(TagCatalogEntry.Create("AUTH", null, CreatedAt)));
    }

    [Fact]
    public async Task FindByNameAsync_returns_normalized_match()
    {
        var repository = CreateRepository(CreateContext());
        await repository.AddAsync(TagCatalogEntry.Create("auth", null, CreatedAt));

        var tag = await repository.FindByNameAsync(" AUTH ");

        Assert.NotNull(tag);
        Assert.Equal("auth", tag.Name);
    }

    private static JsonTagCatalogRepository CreateRepository(TestContext context) =>
        new(context.ProjectRootPath, new JsonFileStore());

    private static TestContext CreateContext()
    {
        var projectRootPath = Path.Combine(
            Path.GetTempPath(),
            "TermBullet.Tests",
            Guid.NewGuid().ToString("N"));
        return new TestContext(
            projectRootPath,
            Path.Combine(projectRootPath, "data", "tags.json"));
    }

    private sealed record TestContext(string ProjectRootPath, string TagsPath);
}
