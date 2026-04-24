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
                "c: add item",
                "x: mark selected item done",
                ">: migrate selected item",
                "q: quit"
            ],
            TuiScreen.DailyFocus =>
            [
                "Tab / Shift+Tab: move panel focus",
                "c: add item into today",
                "Esc: return to dashboard",
                "Use sections to inspect status slices"
            ],
            TuiScreen.WeeklyPlanning =>
            [
                "Tab / Shift+Tab: move panel focus",
                "c: add item into week",
                "Esc: return to dashboard"
            ],
            TuiScreen.BacklogTriage =>
            [
                "Tab / Shift+Tab: move panel focus",
                "c: add item into backlog",
                "/: enter search screen",
                "Esc: return to dashboard"
            ],
            TuiScreen.Review =>
            [
                "Tab / Shift+Tab: move panel focus",
                "c: add item into monthly",
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
            TuiScreen.AddItem =>
            [
                "Type an item and press Enter to add it",
                "Use '-' for tasks, '.' for notes, and 'o' for events",
                "Esc: cancel and return to previous screen",
                "q: quit"
            ],
            _ => ["No contextual help available."]
        };
}
