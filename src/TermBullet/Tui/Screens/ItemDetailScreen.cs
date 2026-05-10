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
        Action onMigrate,
        Action onQuit)
    {
        var topBar = new Label($" TermBullet \u2500 Item {viewModel.PublicRef}")
        {
            X = 0, Y = 0, Width = Dim.Fill()
        };
        var footer = new Label(" e edit  x done  z cancel  > migrate  d delete  Tab/1-5 focus  ? help  Esc back  q quit")
        {
            X = 0, Y = Pos.AnchorEnd(1), Width = Dim.Fill()
        };

        var identityPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(1, "Identity", navigation, 0))
        {
            X = 0, Y = 1, Width = Dim.Percent(50), Height = Dim.Percent(33)
        };
        var planningPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(2, "Planning", navigation, 1))
        {
            X = Pos.Right(identityPanel), Y = 1, Width = Dim.Fill(), Height = Dim.Percent(33)
        };
        var contentPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(3, "Content", navigation, 2))
        {
            X = 0, Y = Pos.Bottom(identityPanel), Width = Dim.Percent(50), Height = Dim.Percent(33)
        };
        var migrationPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(4, "Migration", navigation, 3))
        {
            X = Pos.Right(contentPanel), Y = Pos.Bottom(planningPanel), Width = Dim.Fill(), Height = Dim.Percent(33)
        };
        var historyPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(5, "History", navigation, 4))
        {
            X = 0, Y = Pos.Bottom(contentPanel), Width = Dim.Fill(), Height = Dim.Fill(1)
        };

        var identityList = AddList(identityPanel, viewModel.IdentityLines);
        var planningList = AddList(planningPanel, viewModel.PlanningLines);
        var contentList = AddList(contentPanel, viewModel.ContentLines);
        var migrationList = AddList(migrationPanel, viewModel.MigrationLines);
        var historyList = AddList(historyPanel, viewModel.HistoryLines);

        var panels = new[] { identityPanel, planningPanel, contentPanel, migrationPanel, historyPanel };
        var panelTitles = new[] { "Identity", "Planning", "Content", "Migration", "History" };
        var focusTargets = new View[] { identityList, planningList, contentList, migrationList, historyList };
        TuiScreenUtilities.UpdatePanelTitles(panels, panelTitles, navigation);

        root.Add(topBar, identityPanel, planningPanel, contentPanel, migrationPanel, historyPanel, footer);
        TuiScreenUtilities.FocusCurrentPanel(focusTargets, navigation);

        root.KeyPress += args =>
        {
            if (TuiScreenUtilities.IsHelpKey(args.KeyEvent))
            {
                TuiScreenUtilities.ShowContextHelp(TuiScreen.ItemDetail);
                args.Handled = true;
                return;
            }

            if (TuiScreenUtilities.TryFocusPanelByNumber(args.KeyEvent, navigation, panels, panelTitles, focusTargets))
            {
                args.Handled = true;
                return;
            }

            switch (args.KeyEvent.Key)
            {
                case Key.Tab:
                    navigation.MoveNextPanel();
                    TuiScreenUtilities.UpdatePanelTitles(panels, panelTitles, navigation);
                    TuiScreenUtilities.FocusCurrentPanel(focusTargets, navigation);
                    args.Handled = true;
                    break;
                case Key.BackTab:
                    navigation.MovePreviousPanel();
                    TuiScreenUtilities.UpdatePanelTitles(panels, panelTitles, navigation);
                    TuiScreenUtilities.FocusCurrentPanel(focusTargets, navigation);
                    args.Handled = true;
                    break;
                case Key.Esc:
                    onBack();
                    args.Handled = true;
                    break;
                case Key.q:
                    onQuit();
                    args.Handled = true;
                    break;
                case Key x when x == (Key)'>':
                    onMigrate();
                    args.Handled = true;
                    break;
            }
        };
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
