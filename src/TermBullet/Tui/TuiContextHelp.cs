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
                "Enter: open selected dashboard option",
                "c: add item",
                "x: mark selected item done",
                "z: cancel selected item",
                ">: migrate selected item",
                "d: delete selected item",
                "q: quit"
            ],
            TuiScreen.Search =>
            [
                "Enter in query: execute search",
                "Tab / Shift+Tab: move panel focus",
                "Esc: return to previous screen"
            ],
            _ => ["No contextual help available."]
        };

    public static IReadOnlyList<string> GetAddItemLines() =>
    [
        "Type an item and press Enter to add it",
        "Use '-' for tasks, '.' for notes, and 'o' for events",
        "Esc: cancel and return to previous screen",
        "q: quit"
    ];
}
