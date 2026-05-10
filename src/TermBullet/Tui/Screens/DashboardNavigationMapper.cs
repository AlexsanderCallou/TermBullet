using TermBullet.Tui.Navigation;

namespace TermBullet.Tui.Screens;

public static class DashboardNavigationMapper
{
    public static TuiScreen? FromMenuIndex(int selectedIndex) =>
        selectedIndex switch
        {
            1 => TuiScreen.Search,
            2 => TuiScreen.Planning,
            3 => TuiScreen.Month,
            4 => TuiScreen.Backlog,
            5 => TuiScreen.Forgotten,
            6 => TuiScreen.Notes,
            7 => TuiScreen.Calendar,
            8 => TuiScreen.Tags,
            _ => null
        };

    public static TuiScreen? FromContextIndex(int selectedIndex) =>
        selectedIndex switch
        {
            1 => TuiScreen.MainDashboard,
            2 => TuiScreen.Week,
            3 => TuiScreen.Month,
            4 => TuiScreen.Backlog,
            5 => TuiScreen.Forgotten,
            6 or 7 => TuiScreen.Tags,
            _ => null
        };
}
