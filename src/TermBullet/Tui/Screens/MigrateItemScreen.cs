using Terminal.Gui;
using TermBullet.Domain.Items;
using TermBullet.Tui.Navigation;

namespace TermBullet.Tui.Screens;

public static class MigrateItemScreen
{
    private enum FocusArea
    {
        Destination,
        Save,
        Cancel
    }

    public static void Build(
        View root,
        MigrateItemViewModel viewModel,
        TuiNavigationState navigation,
        Action<MigrateItemViewModel> onViewModelChanged,
        Action<MigrateItemViewModel> onConfirm,
        Action onCancel)
    {
        _ = navigation;
        var currentViewModel = viewModel;
        var focusArea = FocusArea.Destination;
        var syncingDestinationSelection = false;

        var screen = new FrameView($"TermBullet - Migrate {viewModel.Item.PublicRef}")
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(1)
        };
        var footer = new Label(" Enter activate  Tab focus  Space toggle  Esc cancel  ? help")
        {
            X = 0,
            Y = Pos.AnchorEnd(1),
            Width = Dim.Fill()
        };

        AddLines(screen, 1, ["Item", .. viewModel.ItemLines]);
        AddSeparator(screen, 8);

        var destinationTitle = new Label("Destination")
        {
            X = 1,
            Y = 9,
            Width = Dim.Fill(2)
        };
        var destinationGroup = new RadioGroup(["Today", "Week", "Month", "Backlog"], SelectedIndex(currentViewModel.DestinationCollection))
        {
            X = 1,
            Y = 10,
            Width = Dim.Fill(2)
        };

        var resultTitle = new Label("Result")
        {
            X = 1,
            Y = 16,
            Width = Dim.Fill(2)
        };
        var resultLineOne = new Label(currentViewModel.ResultLines.ElementAtOrDefault(0) ?? string.Empty)
        {
            X = 1,
            Y = 17,
            Width = Dim.Fill(2)
        };
        var resultLineTwo = new Label(currentViewModel.ResultLines.ElementAtOrDefault(1) ?? string.Empty)
        {
            X = 1,
            Y = 18,
            Width = Dim.Fill(2)
        };
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

        screen.Add(destinationTitle, destinationGroup, resultTitle, resultLineOne, resultLineTwo, saveButton, cancelButton);
        root.Add(screen, footer);

        void SetFocusArea(FocusArea area)
        {
            focusArea = area;
            switch (area)
            {
                case FocusArea.Destination:
                    destinationGroup.SetFocus();
                    break;
                case FocusArea.Save:
                    saveButton.SetFocus();
                    break;
                case FocusArea.Cancel:
                    cancelButton.SetFocus();
                    break;
            }
        }

        void MoveFocus(int delta)
        {
            FocusArea[] order = [FocusArea.Destination, FocusArea.Save, FocusArea.Cancel];
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

        void RefreshDestination(MigrateItemViewModel updated)
        {
            currentViewModel = updated;
            syncingDestinationSelection = true;
            try
            {
                destinationGroup.SelectedItem = SelectedIndex(updated.DestinationCollection);
            }
            finally
            {
                syncingDestinationSelection = false;
            }

            resultLineOne.Text = updated.ResultLines.ElementAtOrDefault(0) ?? string.Empty;
            resultLineTwo.Text = updated.ResultLines.ElementAtOrDefault(1) ?? string.Empty;
            onViewModelChanged(updated);
        }

        void Submit() => onConfirm(currentViewModel);

        saveButton.Clicked += Submit;
        cancelButton.Clicked += onCancel;
        destinationGroup.SelectedItemChanged += _ =>
        {
            if (syncingDestinationSelection)
            {
                return;
            }

            RefreshDestination(currentViewModel.WithDestination(CollectionFromIndex(destinationGroup.SelectedItem)));
        };

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
                    MoveFocus(1);
                    args.Handled = true;
                    break;
                case Key.BackTab:
                    MoveFocus(-1);
                    args.Handled = true;
                    break;
                case Key.Space when focusArea == FocusArea.Destination:
                    destinationGroup.SelectedItem = destinationGroup.SelectedItem >= 3 ? 0 : destinationGroup.SelectedItem + 1;
                    args.Handled = true;
                    break;
                case Key.Enter:
                    if (focusArea == FocusArea.Save)
                    {
                        Submit();
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

        SetFocusArea(FocusArea.Destination);
    }

    private static int SelectedIndex(ItemCollection collection) =>
        collection switch
        {
            ItemCollection.Today => 0,
            ItemCollection.Week => 1,
            ItemCollection.Month => 2,
            ItemCollection.Backlog => 3,
            _ => 0
        };

    private static ItemCollection CollectionFromIndex(int index) =>
        index switch
        {
            1 => ItemCollection.Week,
            2 => ItemCollection.Month,
            3 => ItemCollection.Backlog,
            _ => ItemCollection.Today
        };

    private static void AddLines(FrameView screen, int startY, IReadOnlyList<string> lines)
    {
        for (var index = 0; index < lines.Count; index++)
        {
            screen.Add(new Label(lines[index])
            {
                X = 1,
                Y = startY + index,
                Width = Dim.Fill(2)
            });
        }
    }

    private static void AddSeparator(FrameView screen, int y)
    {
        screen.Add(new Label(new string('-', 90))
        {
            X = 1,
            Y = y,
            Width = Dim.Fill(2)
        });
    }
}
