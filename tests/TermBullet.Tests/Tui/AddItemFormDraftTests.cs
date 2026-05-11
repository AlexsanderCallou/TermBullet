using TermBullet.Application.Items;
using TermBullet.Domain.Items;
using TermBullet.Tui.Screens;

namespace TermBullet.Tests.Tui;

public sealed class AddItemFormDraftTests
{
    [Fact]
    public void BuildRequest_creates_today_task_with_description_and_tags()
    {
        var draft = new AddItemFormDraft
        {
            Type = ItemType.Task,
            Timing = AddItemTimingChoice.Today,
            Content = "  Fix authentication flow  ",
            Description = "  Keep the CLI and TUI aligned.  ",
            Priority = Priority.High,
            TagsText = " auth, cli ; auth "
        };

        var request = draft.BuildRequest();

        Assert.Equal(ItemType.Task, request.Type);
        Assert.Equal("Fix authentication flow", request.Content);
        Assert.Equal(ItemCollection.Today, request.Collection);
        Assert.Equal("Keep the CLI and TUI aligned.", request.Description);
        Assert.Equal(Priority.High, request.Priority);
        Assert.Equal(["auth", "cli"], request.Tags);
        Assert.Null(request.ScheduledAt);
    }

    [Fact]
    public void BuildRequest_uses_scheduled_at_for_events()
    {
        var draft = new AddItemFormDraft
        {
            Type = ItemType.Event,
            Timing = AddItemTimingChoice.Week,
            Content = "Team sync",
            ScheduledAtText = "2026-05-12"
        };

        var request = draft.BuildRequest();

        Assert.Equal(ItemCollection.Week, request.Collection);
        Assert.Equal(new DateTimeOffset(2026, 5, 12, 0, 0, 0, TimeSpan.Zero), request.ScheduledAt);
    }

    [Fact]
    public void BuildRequest_creates_note_without_planning_fields()
    {
        var draft = new AddItemFormDraft
        {
            Type = ItemType.Note,
            Content = "Investigate stacktrace",
            Description = "Terminal.Gui throws while rendering."
        };

        var request = draft.BuildRequest();

        Assert.Equal(ItemType.Note, request.Type);
        Assert.Equal(ItemCollection.Backlog, request.Collection);
        Assert.Equal(Priority.None, request.Priority);
        Assert.Null(request.ScheduledAt);
        Assert.Equal("Terminal.Gui throws while rendering.", request.Description);
    }

    [Fact]
    public void BuildQuickTaskRequest_creates_today_task_with_only_content()
    {
        var request = AddItemFormDraft.BuildQuickTaskRequest("  Fix auth flow  ");

        Assert.Equal(ItemType.Task, request.Type);
        Assert.Equal("Fix auth flow", request.Content);
        Assert.Equal(ItemCollection.Today, request.Collection);
        Assert.Equal(Priority.None, request.Priority);
        Assert.Null(request.Description);
        Assert.Null(request.Tags);
        Assert.Null(request.ScheduledAt);
    }

    [Fact]
    public void BuildRequest_ignores_priority_for_events()
    {
        var draft = new AddItemFormDraft
        {
            Type = ItemType.Event,
            Content = "Team sync",
            ScheduledAtText = "2026-05-12",
            Priority = Priority.High
        };

        var request = draft.BuildRequest();

        Assert.Equal(Priority.None, request.Priority);
    }

    [Fact]
    public void BuildRequest_rejects_invalid_event_schedule()
    {
        var draft = new AddItemFormDraft
        {
            Type = ItemType.Event,
            Timing = AddItemTimingChoice.Week,
            Content = "Task",
            ScheduledAtText = "not-a-date"
        };

        var exception = Assert.Throws<ArgumentException>(() => draft.BuildRequest());

        Assert.Equal("ScheduledAtText", exception.ParamName);
    }
}
