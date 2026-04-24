using TermBullet.Tui.Navigation;

namespace TermBullet.Tui;

public static class TuiContextHelp
{
    public static IReadOnlyList<string> GetLines(TuiScreen screen) =>
        screen switch
        {
            TuiScreen.MainDashboard =>
            [
                "Tab / Shift+Tab: move panel focus",
                "Enter: open selected collection",
                "c: quick capture",
                "x: mark selected item done",
                ">: migrate selected item"
            ],
            TuiScreen.DailyFocus =>
            [
                "Tab / Shift+Tab: move panel focus",
                "c: quick capture into today",
                "Esc: return to dashboard",
                "Use sections to inspect status slices"
            ],
            TuiScreen.WeeklyPlanning =>
            [
                "Tab / Shift+Tab: move panel focus",
                "c: quick capture into week",
                "Esc: return to dashboard"
            ],
            TuiScreen.BacklogTriage =>
            [
                "Tab / Shift+Tab: move panel focus",
                "c: quick capture into backlog",
                "/: enter search screen",
                "Esc: return to dashboard"
            ],
            TuiScreen.Review =>
            [
                "Tab / Shift+Tab: move panel focus",
                "c: quick capture into monthly",
                "Esc: return to dashboard"
            ],
            TuiScreen.Search =>
            [
                "Enter in query: execute search",
                "Tab / Shift+Tab: move panel focus",
                "Esc: return to previous screen"
            ],
            TuiScreen.Config =>
            [
                "Tab / Shift+Tab: move panel focus",
                "Esc: return to dashboard",
                "Enter: reserved for future config editing"
            ],
            _ => ["No contextual help available."]
        };
}

