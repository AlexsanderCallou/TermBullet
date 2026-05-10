using Terminal.Gui;
using TermBullet.Tui.Navigation;

namespace TermBullet.Tui.Screens;

public static class MigrateItemScreen
{
    private enum FocusArea
    {
        Item,
        Destination,
        Result,
        Save,
        Cancel
    }

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
        var footer = new Label(" Enter activate  Tab/1-3 focus  Space toggle  Esc cancel  ? help")
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
        var saveButton = new Button("Save")
        {
            X = 1,
            Y = Pos.AnchorEnd(2)
        };
        var cancelButton = new Button("Cancel")
        {
            X = Pos.Right(saveButton) + 2,
            Y = Pos.AnchorEnd(2)
        };
        resultPanel.Add(saveButton, cancelButton);

        var panels = new[] { itemPanel, destinationPanel, resultPanel };
        var panelTitles = new[] { "Item", "Destination", "Result" };
        var focusTargets = new View[] { itemList, destinationList, resultList };
        TuiScreenUtilities.UpdatePanelTitles(panels, panelTitles, navigation);

        root.Add(topBar, itemPanel, destinationPanel, resultPanel, footer);
        TuiScreenUtilities.FocusCurrentPanel(focusTargets, navigation);
        var focusArea = navigation.FocusedPanelIndex switch
        {
            1 => FocusArea.Destination,
            2 => FocusArea.Result,
            _ => FocusArea.Item
        };

        void SetFocusArea(FocusArea area)
        {
            focusArea = area;
            switch (area)
            {
                case FocusArea.Item:
                    navigation.FocusPanel(1);
                    TuiScreenUtilities.UpdatePanelTitles(panels, panelTitles, navigation);
                    itemList.SetFocus();
                    break;
                case FocusArea.Destination:
                    navigation.FocusPanel(2);
                    TuiScreenUtilities.UpdatePanelTitles(panels, panelTitles, navigation);
                    destinationList.SetFocus();
                    break;
                case FocusArea.Result:
                    navigation.FocusPanel(3);
                    TuiScreenUtilities.UpdatePanelTitles(panels, panelTitles, navigation);
                    resultList.SetFocus();
                    break;
                case FocusArea.Save:
                    navigation.FocusPanel(3);
                    TuiScreenUtilities.UpdatePanelTitles(panels, panelTitles, navigation);
                    saveButton.SetFocus();
                    break;
                case FocusArea.Cancel:
                    navigation.FocusPanel(3);
                    TuiScreenUtilities.UpdatePanelTitles(panels, panelTitles, navigation);
                    cancelButton.SetFocus();
                    break;
            }
        }

        void MoveFocus(int delta)
        {
            var order = new[]
            {
                FocusArea.Item,
                FocusArea.Destination,
                FocusArea.Result,
                FocusArea.Save,
                FocusArea.Cancel
            };
            var index = Array.IndexOf(order, focusArea);
            index = index < 0 ? 0 : index + delta;
            if (index < 0)
            {
                index = order.Length - 1;
            }
            else if (index >= order.Length)
            {
                index = 0;
            }

            SetFocusArea(order[index]);
        }

        saveButton.Clicked += onConfirm;
        cancelButton.Clicked += onCancel;

        root.KeyPress += args =>
        {
            if (TuiScreenUtilities.IsHelpKey(args.KeyEvent))
            {
                TuiScreenUtilities.ShowContextHelp(TuiScreen.MigrateItem);
                args.Handled = true;
                return;
            }

            if (TuiScreenUtilities.TryFocusPanelByNumber(args.KeyEvent, navigation, panels, panelTitles, focusTargets))
            {
                focusArea = navigation.FocusedPanelIndex switch
                {
                    1 => FocusArea.Destination,
                    2 => FocusArea.Result,
                    _ => FocusArea.Item
                };
                args.Handled = true;
                return;
            }

            switch (args.KeyEvent.Key)
            {
                case Key.Tab:
                    MoveFocus(1);
                    args.Handled = true;
                    break;
                case Key.BackTab:
                    MoveFocus(-1);
                    args.Handled = true;
                    break;
                case Key.Space when focusArea == FocusArea.Destination:
                    onViewModelChanged(viewModel.ToggleDestination());
                    args.Handled = true;
                    break;
                case Key.Enter:
                    if (focusArea == FocusArea.Save)
                    {
                        onConfirm();
                        args.Handled = true;
                    }
                    else if (focusArea == FocusArea.Cancel)
                    {
                        onCancel();
                        args.Handled = true;
                    }
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
