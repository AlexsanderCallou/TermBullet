using TermBullet.Tui.Navigation;
using TermBullet.Tui.Screens;

namespace TermBullet.Tests.Tui;

public sealed class DashboardNavigationMapperTests
{
    [Theory]
    [InlineData(1, TuiScreen.Search)]
    [InlineData(2, TuiScreen.Planning)]
    [InlineData(3, TuiScreen.Backlog)]
    [InlineData(4, TuiScreen.Forgotten)]
    [InlineData(5, TuiScreen.Notes)]
    [InlineData(6, TuiScreen.Calendar)]
    [InlineData(7, TuiScreen.Tags)]
    public void FromMenuIndex_maps_dashboard_menu_entries(int selectedIndex, TuiScreen expected)
    {
        Assert.Equal(expected, DashboardNavigationMapper.FromMenuIndex(selectedIndex));
    }

    [Theory]
    [InlineData(1, TuiScreen.MainDashboard)]
    [InlineData(2, TuiScreen.Week)]
    [InlineData(3, TuiScreen.Backlog)]
    [InlineData(4, TuiScreen.Forgotten)]
    [InlineData(5, TuiScreen.Tags)]
    [InlineData(6, TuiScreen.Tags)]
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
