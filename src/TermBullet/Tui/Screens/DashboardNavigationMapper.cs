using TermBullet.Tui.Navigation;

namespace TermBullet.Tui.Screens;

public static class DashboardNavigationMapper
{
    public static TuiScreen? FromMenuIndex(int selectedIndex) =>
        selectedIndex switch
        {
            1 => TuiScreen.Search,
            2 => TuiScreen.Planning,
            3 => TuiScreen.Backlog,
            4 => TuiScreen.Forgotten,
            5 => TuiScreen.Notes,
            6 => TuiScreen.Calendar,
            7 => TuiScreen.Tags,
            _ => null
        };

    public static TuiScreen? FromContextIndex(int selectedIndex) =>
        selectedIndex switch
        {
            1 => TuiScreen.MainDashboard,
            2 => TuiScreen.Week,
            3 => TuiScreen.Backlog,
            4 => TuiScreen.Forgotten,
            5 or 6 => TuiScreen.Tags,
            _ => null
        };
}
