using Terminal.Gui;

namespace TermBullet.Tui.Screens;

public static class PlanningShortcutPolicy
{
    public static bool IsPromptTextInput(KeyEvent keyEvent) =>
        keyEvent.Key is not Key.Enter
            and not Key.Esc
            and not Key.Tab
            and not Key.BackTab
        && !TuiScreenUtilities.IsHelpKey(keyEvent);
}
