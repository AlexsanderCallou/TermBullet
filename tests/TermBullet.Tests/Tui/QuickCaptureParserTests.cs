using TermBullet.Core.Items;
using TermBullet.Tui;

namespace TermBullet.Tests.Tui;

public sealed class QuickCaptureParserTests
{
    [Fact]
    public void Parse_uses_prefix_to_detect_task_note_and_event()
    {
        var task = QuickCaptureParser.Parse("- review swagger", ItemCollection.Today);
        var note = QuickCaptureParser.Parse(". empty audience bug", ItemCollection.Today);
        var @event = QuickCaptureParser.Parse("o sync 16:00", ItemCollection.Today);

        Assert.Equal(ItemType.Task, task.Type);
        Assert.Equal("review swagger", task.Content);
        Assert.Equal(ItemType.Note, note.Type);
        Assert.Equal("empty audience bug", note.Content);
        Assert.Equal(ItemType.Event, @event.Type);
        Assert.Equal("sync 16:00", @event.Content);
    }

    [Fact]
    public void Parse_defaults_to_task_without_prefix()
    {
        var request = QuickCaptureParser.Parse("fix auth jwt", ItemCollection.Week);

        Assert.Equal(ItemType.Task, request.Type);
        Assert.Equal(ItemCollection.Week, request.Collection);
    }

    [Fact]
    public void Parse_respects_forced_type()
    {
        var request = QuickCaptureParser.Parse(". investigate stacktrace", ItemCollection.Backlog, ItemType.Task);

        Assert.Equal(ItemType.Task, request.Type);
        Assert.Equal("investigate stacktrace", request.Content);
    }

    [Fact]
    public void Parse_throws_for_empty_input()
    {
        Assert.Throws<ArgumentException>(() => QuickCaptureParser.Parse("   ", ItemCollection.Today));
    }
}

