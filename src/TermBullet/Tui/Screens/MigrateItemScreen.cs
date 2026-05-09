using Terminal.Gui;
using TermBullet.Tui.Navigation;

namespace TermBullet.Tui.Screens;

public static class MigrateItemScreen
{
    public static void Build(
        View root,
        MigrateItemViewModel viewModel,
        TuiNavigationState navigation,
        Action<MigrateItemViewModel> onViewModelChanged,
        Action onConfirm,
        Action onCancel)
    {
        var topBar = new Label($" TermBullet \u2500 Migrate {viewModel.Item.PublicRef}")
        {
            X = 0, Y = 0, Width = Dim.Fill()
        };
        var footer = new Label(" Enter migrate  Tab focus  Space toggle  Esc cancel  ? help")
        {
            X = 0, Y = Pos.AnchorEnd(1), Width = Dim.Fill()
        };

        var itemPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(1, "Item", navigation, 0))
        {
            X = 0, Y = 1, Width = Dim.Fill(), Height = Dim.Percent(40)
        };
        var destinationPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(2, "Destination", navigation, 1))
        {
            X = 0, Y = Pos.Bottom(itemPanel), Width = Dim.Percent(50), Height = Dim.Fill(1)
        };
        var resultPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(3, "Result", navigation, 2))
        {
            X = Pos.Right(destinationPanel), Y = Pos.Bottom(itemPanel), Width = Dim.Fill(), Height = Dim.Fill(1)
        };

        var itemList = AddList(itemPanel, viewModel.ItemLines);
        var destinationList = AddList(destinationPanel, viewModel.DestinationLines);
        var resultList = AddList(resultPanel, viewModel.ResultLines);

        var panels = new[] { itemPanel, destinationPanel, resultPanel };
        var panelTitles = new[] { "Item", "Destination", "Result" };
        var focusTargets = new View[] { itemList, destinationList, resultList };
        TuiScreenUtilities.UpdatePanelTitles(panels, panelTitles, navigation);

        root.Add(topBar, itemPanel, destinationPanel, resultPanel, footer);
        TuiScreenUtilities.FocusCurrentPanel(focusTargets, navigation);

        root.KeyPress += args =>
        {
            if (TuiScreenUtilities.IsHelpKey(args.KeyEvent))
            {
                TuiScreenUtilities.ShowContextHelp(TuiScreen.MigrateItem);
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
                case Key.Space:
                    onViewModelChanged(viewModel.ToggleDestination());
                    args.Handled = true;
                    break;
                case Key.Enter:
                    onConfirm();
                    args.Handled = true;
                    break;
                case Key.Esc:
                    onCancel();
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
