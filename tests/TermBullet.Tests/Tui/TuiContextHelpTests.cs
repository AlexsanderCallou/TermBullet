using TermBullet.Tui;
using TermBullet.Tui.Navigation;

namespace TermBullet.Tests.Tui;

public sealed class TuiContextHelpTests
{
    [Fact]
    public void MainDashboard_help_contains_capture_and_navigation_actions()
    {
        var lines = TuiContextHelp.GetLines(TuiScreen.MainDashboard);

        Assert.Contains(lines, line => line.Contains("quick capture", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(lines, line => line.Contains("move panel focus", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Search_help_contains_search_specific_shortcuts()
    {
        var lines = TuiContextHelp.GetLines(TuiScreen.Search);

        Assert.Contains(lines, line => line.Contains("execute search", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(lines, line => line.Contains("return to previous screen", StringComparison.OrdinalIgnoreCase));
    }
}
