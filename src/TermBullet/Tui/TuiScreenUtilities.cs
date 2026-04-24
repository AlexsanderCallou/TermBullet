using Terminal.Gui;
using TermBullet.Tui.Navigation;

namespace TermBullet.Tui;

public static class TuiScreenUtilities
{
    public static bool IsHelpKey(KeyEvent keyEvent) =>
        keyEvent.Key == (Key)'?'
        || keyEvent.KeyValue == '?'
        || (keyEvent.KeyValue == '/' && keyEvent.IsShift);

    public static string GetPanelTitle(int number, string title, TuiNavigationState navigation, int panelIndex) =>
        navigation.IsPanelFocused(panelIndex)
            ? $"> {number} {title}"
            : $"{number} {title}";

    public static void RefreshListView(ListView listView, IReadOnlyList<string> items)
    {
        listView.SetSource(items.ToList());
    }

    public static void UpdatePanelTitles(
        IReadOnlyList<FrameView> panels,
        IReadOnlyList<string> titles,
        TuiNavigationState navigation)
    {
        for (var index = 0; index < panels.Count && index < titles.Count; index++)
        {
            panels[index].Title = GetPanelTitle(index + 1, titles[index], navigation, index);
        }
    }

    public static void FocusCurrentPanel(
        IReadOnlyList<View> focusTargets,
        TuiNavigationState navigation)
    {
        if (navigation.FocusedPanelIndex < 0 || navigation.FocusedPanelIndex >= focusTargets.Count)
        {
            return;
        }

        focusTargets[navigation.FocusedPanelIndex].SetFocus();
    }

    public static void ShowContextHelp(TuiScreen screen)
    {
        var lines = string.Join(Environment.NewLine, TuiContextHelp.GetLines(screen));
        MessageBox.Query("Help", lines, "Close");
    }

    public static void ShowAddItemHelp()
    {
        var lines = string.Join(Environment.NewLine, TuiContextHelp.GetAddItemLines());
        MessageBox.Query("Help", lines, "Close");
    }
}
