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
                "t: open tags",
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
                "Destination: choose Today, Week, Month, or Backlog"
            ],
            TuiScreen.Planning =>
            [
                "Planning creates new project drafts from guided topic, tag, volume, and today choices",
                "s: cycle Small, Medium, and Large task volume",
                "t: toggle first task today",
                "g: generate a structured planning draft",
                "AI drafts must be approved before applying",
                "a: apply current draft",
                "d: discard current draft",
                "Esc: return to previous screen",
                "q: quit"
            ],
            TuiScreen.Week =>
            [
                "Tab / Shift+Tab or 1-9: move between Week, Preview, and Actions",
                "Week is a task collection, not a dated schedule",
                "Enter: open selected item detail",
                ">: migrate selected task",
                "x / z / d: mark done, cancel, or delete selected item",
                "Esc: return to previous screen"
            ],
            TuiScreen.Backlog =>
            [
                "Tab / Shift+Tab or 1-9: move between Backlog, Preview, and Actions",
                "Enter: open selected item detail",
                ">: migrate selected task",
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
                "z / d: cancel or delete selected event",
                "Esc: return to previous screen"
            ],
            TuiScreen.Tags =>
            [
                "Tab / Shift+Tab or 1-4: move between Search, Tags, Preview, and Actions",
                "Enter: open selected tag detail",
                "n: open Create Tag flow",
                "Search filters only tags in this screen",
                "Default is protected and always available",
                "Esc: return to previous screen"
            ],
            TuiScreen.TagDetail =>
            [
                "Tab / Shift+Tab or 1-5: move between Summary, Timeline, Tasks, Notes, and Events",
                "Enter: open selected item detail",
                "c: create item with this tag",
                "n: quick task with this tag",
                "e: edit selected item",
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
