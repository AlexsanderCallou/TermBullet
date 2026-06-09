using Terminal.Gui;

namespace TermBullet.Tui.Screens;

public static class PlanningShortcutPolicy
{
    public static bool IsPromptTextInput(KeyEvent keyEvent) =>
        TuiScreenUtilities.IsTextInputOwnedKey(keyEvent);
}
