using TermBullet.Tui.Navigation;

namespace TermBullet.Tui;

public static class TuiContextHelp
{
    public static IReadOnlyList<string> GetLines(TuiScreen screen) =>
        screen switch
        {
            TuiScreen.MainDashboard =>
            [
                "Tab / Shift+Tab or 1-9: move panel focus",
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
                "Tab / Shift+Tab or 1-9: move panel focus",
                "Esc: return to previous screen"
            ],
            TuiScreen.ItemDetail =>
            [
                "Tab / Shift+Tab or 1-9: move panel focus",
                ">: open migration flow",
                "History: review loaded item history when available",
                "Esc: return to previous screen"
            ],
            TuiScreen.MigrateItem =>
            [
                "Tab / Shift+Tab or 1-9: move panel focus",
                "Space: toggle destination",
                "Enter: activate focused control",
                "Save: confirm migration",
                "Cancel / Esc: cancel migration",
                "Destination: choose Date or Backlog"
            ],
            TuiScreen.Planning =>
            [
                "Planning is a future AI-assisted workspace",
                "V1 keeps this screen empty and local-first",
                "Esc: return to previous screen",
                "q: quit"
            ],
            TuiScreen.Week =>
            [
                "Tab / Shift+Tab or 1-9: move between weekday panels and preview",
                "Enter: open selected item detail",
                ">: migrate selected task",
                "x / z / d: mark done, cancel, or delete selected item",
                "Esc: return to previous screen"
            ],
            TuiScreen.Backlog =>
            [
                "Tab / Shift+Tab or 1-9: move between Backlog, Preview, and Actions",
                "Enter: open selected item detail",
                ">: plan selected task",
                "x / z / d: mark done, cancel, or delete selected item",
                "Esc: return to previous screen"
            ],
            TuiScreen.Forgotten =>
            [
                "Tab / Shift+Tab or 1-9: move between Items, Preview, and Resolution",
                "Enter: open selected item detail",
                ">: migrate selected forgotten task",
                "x / z / d: mark done, cancel, or delete selected task",
                "Esc: return to previous screen"
            ],
            TuiScreen.Notes =>
            [
                "Tab / Shift+Tab or 1-9: move between Notes, Preview, and Actions",
                "Enter: open selected note detail",
                "d: delete selected note",
                "Esc: return to previous screen"
            ],
            TuiScreen.Calendar =>
            [
                "Tab / Shift+Tab or 1-9: move between Month, Day Items, Preview, and Actions",
                "Arrows: move selected day",
                "[ / ]: move month",
                "Enter: open selected dated item detail",
                "x / z / d: mark done, cancel, or delete selected item",
                "Esc: return to previous screen"
            ],
            TuiScreen.Tags =>
            [
                "Tab / Shift+Tab or 1-9: move between Tags, Preview, and Actions",
                "Enter: preview selected tag",
                "c: open Create Tag flow",
                "Tags are derived from item metadata in V1",
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
        "Enter: activate focused control",
        "Save: add the item",
        "Cancel / Esc: return without saving",
        "Task uses collections; Event uses scheduled_at; Note has no planning date",
        "q: quit"
    ];
}
