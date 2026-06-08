using TermBullet.Tui.Navigation;
using TermBullet.Tui.Screens;

namespace TermBullet.Tests.Tui;

public sealed class DashboardNavigationMapperTests
{
    [Theory]
    [InlineData(1, TuiScreen.Search)]
    [InlineData(2, TuiScreen.Planning)]
    [InlineData(3, TuiScreen.Month)]
    [InlineData(4, TuiScreen.Backlog)]
    [InlineData(5, TuiScreen.DailyReview)]
    [InlineData(6, TuiScreen.Forgotten)]
    [InlineData(7, TuiScreen.Notes)]
    [InlineData(8, TuiScreen.Calendar)]
    [InlineData(9, TuiScreen.Tags)]
    public void FromMenuIndex_maps_dashboard_menu_entries(int selectedIndex, TuiScreen expected)
    {
        Assert.Equal(expected, DashboardNavigationMapper.FromMenuIndex(selectedIndex));
    }

    [Theory]
    [InlineData(1, TuiScreen.MainDashboard)]
    [InlineData(2, TuiScreen.DailyReview)]
    [InlineData(3, TuiScreen.Week)]
    [InlineData(4, TuiScreen.Month)]
    [InlineData(5, TuiScreen.Backlog)]
    [InlineData(6, TuiScreen.Forgotten)]
    [InlineData(7, TuiScreen.Tags)]
    [InlineData(8, TuiScreen.Tags)]
    public void FromContextIndex_maps_dashboard_context_entries(int selectedIndex, TuiScreen expected)
    {
        Assert.Equal(expected, DashboardNavigationMapper.FromContextIndex(selectedIndex));
    }

    [Fact]
    public void FromContextIndex_ignores_headers()
    {
        Assert.Null(DashboardNavigationMapper.FromContextIndex(0));
    }
}
