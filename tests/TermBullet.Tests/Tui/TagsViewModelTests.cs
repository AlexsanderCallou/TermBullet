using TermBullet.Tui.Screens;
using TermBullet.Application.Tags;

namespace TermBullet.Tests.Tui;

public sealed class TagsViewModelTests
{
    [Fact]
    public void Build_summarizes_tag_usage_by_item_type()
    {
        var rows = new[]
        {
            MakeRow("t-0526-1", "task", "open", "auth"),
            MakeRow("t-0526-2", "task", "done", "auth"),
            MakeRow("n-0526-1", "note", "open", "auth"),
            MakeRow("e-0526-1", "event", "open", "calendar")
        };

        var vm = TagsViewModel.Build([], rows);

        var auth = Assert.Single(vm.Tags, tag => tag.Name == "auth");
        Assert.Equal(3, auth.UsageCount);
        Assert.Equal(1, auth.ActiveTaskCount);
        Assert.Equal(1, auth.NoteCount);
        Assert.Equal(0, auth.EventCount);
    }

    [Fact]
    public void Build_includes_cataloged_tags_without_usage()
    {
        var catalog = new[]
        {
            new TagCatalogResult(
                "auth",
                "Authentication work",
                new DateTimeOffset(2026, 5, 1, 8, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 5, 1, 8, 0, 0, TimeSpan.Zero))
        };

        var vm = TagsViewModel.Build(catalog, []);

        var tag = Assert.Single(vm.Tags);
        Assert.Equal("auth", tag.Name);
        Assert.Equal("Authentication work", tag.Description);
        Assert.True(tag.IsCataloged);
        Assert.Equal(0, tag.UsageCount);
    }

    private static ItemDisplayRow MakeRow(
        string publicRef,
        string type,
        string status,
        string tag) =>
        new()
        {
            Id = Guid.NewGuid(),
            PublicRef = publicRef,
            Symbol = type == "note" ? "(.)" : type == "event" ? "(o)" : "[ ]",
            Type = type,
            Status = status,
            Content = "Item",
            Description = null,
            Priority = "none",
            Collection = "today",
            Tag = tag,
            ScheduledAt = null,
            Version = 1,
            CreatedAt = new DateTimeOffset(2026, 5, 1, 8, 0, 0, TimeSpan.Zero),
            UpdatedAt = new DateTimeOffset(2026, 5, 2, 8, 0, 0, TimeSpan.Zero)
        };
}
