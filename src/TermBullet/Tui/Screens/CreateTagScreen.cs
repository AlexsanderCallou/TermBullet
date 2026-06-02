using Terminal.Gui;
using TermBullet.Tui.Navigation;

namespace TermBullet.Tui.Screens;

public static class CreateTagScreen
{
    private enum FocusArea
    {
        Name,
        Description,
        Preview,
        Save,
        Cancel
    }

    public static void Build(View root, string? error, Action<string, string?> onSubmit, Action onCancel)
    {
        var topBar = new Label(" TermBullet - Create Tag")
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill()
        };
        var namePanel = new FrameView("1 Name")
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill(),
            Height = 4
        };
        var nameField = new TextField(string.Empty)
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill(2)
        };
        namePanel.Add(nameField);

        var descriptionPanel = new FrameView("2 Description")
        {
            X = 0,
            Y = Pos.Bottom(namePanel),
            Width = Dim.Fill(),
            Height = 6
        };
        var descriptionField = new TextView
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill(2),
            Height = Dim.Fill(1)
        };
        descriptionPanel.Add(descriptionField);

        var previewPanel = new FrameView("3 Preview")
        {
            X = 0,
            Y = Pos.Bottom(descriptionPanel),
            Width = Dim.Fill(),
            Height = 6
        };
        var previewList = new ListView(TuiScreenUtilities.SanitizeListItems(BuildPreviewLines(null, null, error)))
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        previewPanel.Add(previewList);

        var saveButton = new Button("Save")
        {
            X = 0,
            Y = Pos.Bottom(previewPanel) + 1
        };
        var cancelButton = new Button("Cancel")
        {
            X = Pos.Right(saveButton) + 2,
            Y = Pos.Bottom(previewPanel) + 1
        };

        var footer = new Label(" Enter activate  Tab focus  Esc cancel  ? help")
        {
            X = 0,
            Y = Pos.AnchorEnd(1),
            Width = Dim.Fill()
        };

        root.Add(topBar, namePanel, descriptionPanel, previewPanel, saveButton, cancelButton, footer);

        var focusArea = FocusArea.Name;

        void RefreshPreview()
        {
            TuiScreenUtilities.RefreshListView(
                previewList,
                BuildPreviewLines(
                    nameField.Text?.ToString(),
                    descriptionField.Text?.ToString(),
                    error));
        }

        void Submit()
        {
            onSubmit(
                nameField.Text?.ToString() ?? string.Empty,
                descriptionField.Text?.ToString());
        }

        void SetFocusArea(FocusArea area)
        {
            focusArea = area;
            switch (area)
            {
                case FocusArea.Name:
                    nameField.SetFocus();
                    break;
                case FocusArea.Description:
                    descriptionField.SetFocus();
                    break;
                case FocusArea.Preview:
                    previewList.SetFocus();
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
            var order = new[]
            {
                FocusArea.Name,
                FocusArea.Description,
                FocusArea.Preview,
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

        saveButton.Clicked += Submit;
        cancelButton.Clicked += onCancel;

        void AttachTextNavigation(View view)
        {
            view.KeyPress += args =>
            {
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
                }

                RefreshPreview();
            };
        }

        AttachTextNavigation(nameField);
        AttachTextNavigation(descriptionField);

        root.KeyPress += args =>
        {
            if (TuiScreenUtilities.IsHelpKey(args.KeyEvent))
            {
                TuiScreenUtilities.ShowContextHelp(TuiScreen.Tags);
                args.Handled = true;
                return;
            }

            var digit = TuiScreenUtilities.GetDigit(args.KeyEvent);
            if (digit is not null)
            {
                var target = digit.Value switch
                {
                    1 => FocusArea.Name,
                    2 => FocusArea.Description,
                    3 => FocusArea.Preview,
                    _ => (FocusArea?)null
                };

                if (target is not null)
                {
                    SetFocusArea(target.Value);
                    args.Handled = true;
                    return;
                }
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

            RefreshPreview();
        };

        SetFocusArea(FocusArea.Name);
    }

    private static string[] BuildPreviewLines(string? name, string? description, string? error) =>
    [
        $"name: {(string.IsNullOrWhiteSpace(name) ? "(required)" : name.Trim())}",
        $"description: {(string.IsNullOrWhiteSpace(description) ? "-" : description.Trim())}",
        string.IsNullOrWhiteSpace(error) ? "status: ready to create" : $"status: {error}"
    ];
}
