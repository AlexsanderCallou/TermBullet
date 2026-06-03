using TermBullet.Tui.Screens;

namespace TermBullet.Tests.Tui;

public sealed class PlanningConversationFormatterTests
{
    [Fact]
    public void Format_wraps_long_conversation_messages()
    {
        var lines = PlanningConversationFormatter.Format(
            [
                "assistant> This is a long planning response with enough detail to require wrapping inside the TUI conversation panel."
            ],
            currentDraft: null,
            maxLineLength: 40);

        Assert.True(lines.Count > 1);
        Assert.All(lines, line => Assert.True(line.Length <= 40, line));
        Assert.StartsWith("assistant>", lines[0], StringComparison.Ordinal);
        Assert.StartsWith("          ", lines[1], StringComparison.Ordinal);
    }
}
