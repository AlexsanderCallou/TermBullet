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
                "c: choose item type to add",
                "n: quick task for today",
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
            TuiScreen.ItemDetail =>
            [
                "Tab / Shift+Tab: move panel focus",
                ">: open migration flow",
                "History: review loaded item history when available",
                "Esc: return to previous screen"
            ],
            TuiScreen.MigrateItem =>
            [
                "Space: toggle destination",
                "Enter: confirm migration",
                "Esc: cancel migration",
                "Destination: choose Date or Backlog"
            ],
            TuiScreen.Week =>
            [
                "Tab / Shift+Tab: move between weekday panels and preview",
                "Enter: open selected item detail",
                ">: migrate selected task",
                "x / z / d: mark done, cancel, or delete selected item",
                "Esc: return to previous screen"
            ],
            TuiScreen.Backlog =>
            [
                "Tab / Shift+Tab: move between Backlog, Preview, and Actions",
                "Enter: open selected item detail",
                ">: plan selected task",
                "x / z / d: mark done, cancel, or delete selected item",
                "Esc: return to previous screen"
            ],
            TuiScreen.Forgotten =>
            [
                "Tab / Shift+Tab: move between Items, Preview, and Resolution",
                "Enter: open selected item detail",
                ">: migrate selected forgotten task",
                "x / z / d: mark done, cancel, or delete selected task",
                "Esc: return to previous screen"
            ],
            _ => ["No contextual help available."]
        };

    public static IReadOnlyList<string> GetAddItemLines() =>
    [
        "Tab / Shift+Tab: move between fields",
        "c: open the item type picker from the dashboard",
        "n: quick task for today from the dashboard",
        "CursorUp / CursorDown: change the selected timing option",
        "Space: also change the selected timing option",
        "Enter: add the item",
        "Task uses planned_for; Event uses scheduled_at; Note has no planning date",
        "Esc: cancel and return to previous screen",
        "q: quit"
    ];
}
