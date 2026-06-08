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
            5 => TuiScreen.DailyReview,
            6 => TuiScreen.Forgotten,
            7 => TuiScreen.Notes,
            8 => TuiScreen.Calendar,
            9 => TuiScreen.Tags,
            _ => null
        };

    public static TuiScreen? FromContextIndex(int selectedIndex) =>
        selectedIndex switch
        {
            1 => TuiScreen.MainDashboard,
            2 => TuiScreen.DailyReview,
            3 => TuiScreen.Week,
            4 => TuiScreen.Month,
            5 => TuiScreen.Backlog,
            6 => TuiScreen.Forgotten,
            7 or 8 => TuiScreen.Tags,
            _ => null
        };
}
