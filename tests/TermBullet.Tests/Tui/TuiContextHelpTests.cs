using TermBullet.Tui;
using TermBullet.Tui.Navigation;

namespace TermBullet.Tests.Tui;

public sealed class TuiContextHelpTests
{
    [Fact]
    public void MainDashboard_help_contains_capture_and_navigation_actions()
    {
        var lines = TuiContextHelp.GetLines(TuiScreen.MainDashboard);

        Assert.Contains(lines, line => line.Contains("choose item type", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(lines, line => line.Contains("quick task", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(lines, line => line.Contains("move panel focus", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void AddItem_help_contains_submit_and_cancel_actions()
    {
        var lines = TuiContextHelp.GetAddItemLines();

        Assert.Contains(lines, line => line.Contains("Tab / Shift+Tab", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(lines, line => line.Contains("Space", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(lines, line => line.Contains("Enter", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(lines, line => line.Contains("Esc", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Search_help_contains_search_specific_shortcuts()
    {
        var lines = TuiContextHelp.GetLines(TuiScreen.Search);

        Assert.Contains(lines, line => line.Contains("execute search", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(lines, line => line.Contains("return to previous screen", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ItemDetail_help_contains_history_and_back_actions()
    {
        var lines = TuiContextHelp.GetLines(TuiScreen.ItemDetail);

        Assert.Contains(lines, line => line.Contains("history", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(lines, line => line.Contains("Esc", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void MigrateItem_help_contains_destination_and_confirm_actions()
    {
        var lines = TuiContextHelp.GetLines(TuiScreen.MigrateItem);

        Assert.Contains(lines, line => line.Contains("destination", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(lines, line => line.Contains("confirm", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Planning_help_describes_future_ai_workspace()
    {
        var lines = TuiContextHelp.GetLines(TuiScreen.Planning);

        Assert.Contains(lines, line => line.Contains("future AI-assisted workspace", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(lines, line => line.Contains("empty", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData(TuiScreen.Week, "weekday")]
    [InlineData(TuiScreen.Backlog, "Backlog")]
    [InlineData(TuiScreen.Forgotten, "forgotten")]
    public void Planning_list_help_contains_navigation_and_action_shortcuts(TuiScreen screen, string expectedContext)
    {
        var lines = TuiContextHelp.GetLines(screen);

        Assert.Contains(lines, line => line.Contains(expectedContext, StringComparison.OrdinalIgnoreCase));
        Assert.Contains(lines, line => line.Contains("Enter", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(lines, line => line.Contains("x / z / d", StringComparison.OrdinalIgnoreCase));
    }
}
