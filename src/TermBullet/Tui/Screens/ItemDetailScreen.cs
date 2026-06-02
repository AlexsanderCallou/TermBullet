using Terminal.Gui;
using TermBullet.Tui.Navigation;

namespace TermBullet.Tui.Screens;

public static class ItemDetailScreen
{
    public static void Build(
        View root,
        ItemDetailViewModel viewModel,
        TuiNavigationState navigation,
        Action onBack,
        Action onEdit,
        Action onQuit)
    {
        var topBar = new Label($" TermBullet - {viewModel.DetailTitle}")
        {
            X = 0, Y = 0, Width = Dim.Fill()
        };
        var footer = new Label(" e edit  Tab/1-3 focus  ? help  Esc back  q quit")
        {
            X = 0, Y = Pos.AnchorEnd(1), Width = Dim.Fill()
        };

        var topHeight = Dim.Percent(30);
        var summaryPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(1, viewModel.SummaryTitle, navigation, 0))
        {
            X = 0, Y = 1, Width = Dim.Percent(50), Height = topHeight
        };
        var historyPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(2, "History", navigation, 1))
        {
            X = Pos.Right(summaryPanel), Y = 1, Width = Dim.Fill(), Height = topHeight
        };
        var contentPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(3, "Content", navigation, 2))
        {
            X = 0, Y = Pos.Bottom(summaryPanel), Width = Dim.Fill(), Height = Dim.Fill(1)
        };

        var summaryList = AddList(summaryPanel, viewModel.SummaryLines);
        var historyList = AddList(historyPanel, viewModel.HistoryLines);
        var contentList = AddList(contentPanel, viewModel.ContentLines);

        var panels = new[] { summaryPanel, historyPanel, contentPanel };
        var panelTitles = new[] { viewModel.SummaryTitle, "History", "Content" };
        var focusTargets = new View[] { summaryList, historyList, contentList };
        TuiScreenUtilities.UpdatePanelTitles(panels, panelTitles, navigation);

        root.Add(topBar, summaryPanel, historyPanel, contentPanel, footer);
        TuiScreenUtilities.FocusCurrentPanel(focusTargets, navigation);

        bool HandleDetailShortcut(KeyEvent keyEvent)
        {
            if (TuiScreenUtilities.IsHelpKey(keyEvent))
            {
                TuiScreenUtilities.ShowContextHelp(TuiScreen.ItemDetail);
                return true;
            }

            if (TuiScreenUtilities.TryFocusPanelByNumber(keyEvent, navigation, panels, panelTitles, focusTargets))
            {
                return true;
            }

            switch (keyEvent.Key)
            {
                case Key.Tab:
                    navigation.MoveNextPanel();
                    TuiScreenUtilities.UpdatePanelTitles(panels, panelTitles, navigation);
                    TuiScreenUtilities.FocusCurrentPanel(focusTargets, navigation);
                    return true;
                case Key.BackTab:
                    navigation.MovePreviousPanel();
                    TuiScreenUtilities.UpdatePanelTitles(panels, panelTitles, navigation);
                    TuiScreenUtilities.FocusCurrentPanel(focusTargets, navigation);
                    return true;
                case Key.Esc:
                    onBack();
                    return true;
                case Key.q:
                    onQuit();
                    return true;
                case Key e when e == (Key)'e':
                    onEdit();
                    return true;
            }

            return false;
        }

        root.KeyPress += args =>
        {
            if (HandleDetailShortcut(args.KeyEvent))
            {
                args.Handled = true;
            }
        };

        foreach (var target in focusTargets)
        {
            target.KeyPress += args =>
            {
                if (HandleDetailShortcut(args.KeyEvent))
                {
                    args.Handled = true;
                }
            };
        }
    }

    private static ListView AddList(FrameView panel, IReadOnlyList<string> lines)
    {
        var list = new ListView(TuiScreenUtilities.SanitizeListItems(lines))
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };
        panel.Add(list);
        return list;
    }
}
