using Terminal.Gui;

namespace TermBullet.Tui;

public static class TuiItemActionShortcutMapper
{
    public static bool TryMap(Key key, out TuiItemActionShortcut action)
    {
        action = key switch
        {
            Key x when x == (Key)'x' => TuiItemActionShortcut.Done,
            Key z when z == (Key)'z' => TuiItemActionShortcut.Cancel,
            Key m when m == (Key)'>' => TuiItemActionShortcut.Migrate,
            Key d when d == (Key)'d' => TuiItemActionShortcut.Delete,
            _ => (TuiItemActionShortcut)(-1)
        };

        return Enum.IsDefined(action);
    }
}
