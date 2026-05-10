using Terminal.Gui;
using TermBullet.Tui.Navigation;

namespace TermBullet.Tui;

public static class TuiScreenUtilities
{
    public static bool IsHelpKey(KeyEvent keyEvent) =>
        keyEvent.Key == (Key)'?'
        || keyEvent.KeyValue == '?'
        || (keyEvent.KeyValue == '/' && keyEvent.IsShift);

    public static bool TryHandleEnter(Key key, Action action)
    {
        if (key != Key.Enter)
        {
            return false;
        }

        action();
        return true;
    }

    public static string GetPanelTitle(int number, string title, TuiNavigationState navigation, int panelIndex) =>
        navigation.IsPanelFocused(panelIndex)
            ? $"> {number} {title}"
            : $"{number} {title}";

    public static void RefreshListView(ListView listView, IReadOnlyList<string> items)
    {
        listView.SetSource(SanitizeListItems(items));
    }

    public static List<string> SanitizeListItems(IReadOnlyList<string> items)
    {
        if (items.Count == 0)
        {
            return [" "];
        }

        return items
            .Select(item => string.IsNullOrWhiteSpace(item) ? " " : item)
            .ToList();
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

    public static int? GetDigit(KeyEvent keyEvent)
    {
        if (keyEvent.KeyValue >= '1' && keyEvent.KeyValue <= '9')
        {
            return keyEvent.KeyValue - '0';
        }

        return keyEvent.Key switch
        {
            var key when key == (Key)'1' => 1,
            var key when key == (Key)'2' => 2,
            var key when key == (Key)'3' => 3,
            var key when key == (Key)'4' => 4,
            var key when key == (Key)'5' => 5,
            var key when key == (Key)'6' => 6,
            var key when key == (Key)'7' => 7,
            var key when key == (Key)'8' => 8,
            var key when key == (Key)'9' => 9,
            _ => null
        };
    }

    public static bool TryFocusPanelByNumber(
        KeyEvent keyEvent,
        TuiNavigationState navigation,
        IReadOnlyList<FrameView> panels,
        IReadOnlyList<string> titles,
        IReadOnlyList<View> focusTargets)
    {
        var panelNumber = GetDigit(keyEvent);
        if (panelNumber is null)
        {
            return false;
        }

        if (!navigation.FocusPanel(panelNumber.Value))
        {
            return false;
        }

        UpdatePanelTitles(panels, titles, navigation);
        FocusCurrentPanel(focusTargets, navigation);
        return true;
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
