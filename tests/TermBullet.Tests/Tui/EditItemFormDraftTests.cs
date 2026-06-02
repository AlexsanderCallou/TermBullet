using TermBullet.Tui.Screens;

namespace TermBullet.Tests.Tui;

public sealed class EditItemFormDraftTests
{
    [Fact]
    public void BuildPreviewLines_omits_note_collection_priority_and_schedule_noise()
    {
        var draft = new EditItemFormDraft
        {
            PublicRef = "n-0526-1",
            Type = TermBullet.Domain.Items.ItemType.Note,
            Content = "OAuth notes",
            SelectedTag = "auth"
        };

        var lines = draft.BuildPreviewLines();

        Assert.DoesNotContain(lines, line => line.Contains("collection:", StringComparison.Ordinal));
        Assert.DoesNotContain(lines, line => line.Contains("priority:", StringComparison.Ordinal));
        Assert.DoesNotContain(lines, line => line.Contains("scheduled_at:", StringComparison.Ordinal));
        Assert.Contains(lines, line => line.Contains("tag: auth", StringComparison.Ordinal));
    }

    [Fact]
    public void BuildPreviewLines_omits_event_collection_and_priority_noise()
    {
        var draft = new EditItemFormDraft
        {
            PublicRef = "e-0526-1",
            Type = TermBullet.Domain.Items.ItemType.Event,
            Content = "Dentist appointment",
            ScheduledAtText = "2026-05-12"
        };

        var lines = draft.BuildPreviewLines();

        Assert.Contains(lines, line => line.Contains("scheduled_at: 2026-05-12", StringComparison.Ordinal));
        Assert.DoesNotContain(lines, line => line.Contains("collection:", StringComparison.Ordinal));
        Assert.DoesNotContain(lines, line => line.Contains("priority:", StringComparison.Ordinal));
    }
}
