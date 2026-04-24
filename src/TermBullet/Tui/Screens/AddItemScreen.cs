using Terminal.Gui;
using TermBullet.Tui.Navigation;

namespace TermBullet.Tui.Screens;

public static class AddItemScreen
{
    public static void Build(
        View root,
        TuiAddItemViewModel viewModel,
        Action<string> onSubmit,
        Action onCancel,
        Action onQuit)
    {
        var topBar = new Label($" TermBullet - Add Item - target:{viewModel.Collection.ToString().ToLowerInvariant()}")
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill()
        };

        var footer = new Label(" Enter add  Esc cancel  ? help  q quit")
        {
            X = 0,
            Y = Pos.AnchorEnd(1),
            Width = Dim.Fill()
        };

        var formPanel = new FrameView("Add")
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Percent(45)
        };

        var inputLabel = new Label("Item:")
        {
            X = 1,
            Y = 1,
            Width = 8
        };

        var input = new TextField(string.Empty)
        {
            X = Pos.Right(inputLabel) + 1,
            Y = 1,
            Width = Dim.Fill(2)
        };

        var error = new Label(viewModel.Error is null ? string.Empty : $"Error: {viewModel.Error}")
        {
            X = 1,
            Y = 3,
            Width = Dim.Fill(2)
        };

        formPanel.Add(inputLabel, input, error);

        var examplesPanel = new FrameView("Examples")
        {
            X = 0,
            Y = Pos.Bottom(formPanel),
            Width = Dim.Fill(),
            Height = Dim.Fill(1)
        };

        var exampleLines = new List<string>
        {
            "Prefixes:",
            "  - task",
            "  . note",
            "  o event",
            string.Empty,
            "Examples:"
        };
        exampleLines.AddRange(viewModel.Examples.Select(example => $"  {example}"));

        for (var index = 0; index < exampleLines.Count; index++)
        {
            examplesPanel.Add(new Label(exampleLines[index])
            {
                X = 1,
                Y = index,
                Width = Dim.Fill(2)
            });
        }

        root.Add(topBar, formPanel, examplesPanel, footer);

        input.KeyPress += args =>
        {
            if (args.KeyEvent.Key == Key.Enter)
            {
                onSubmit(input.Text?.ToString() ?? string.Empty);
                args.Handled = true;
            }
        };

        root.KeyPress += args =>
        {
            if (TuiScreenUtilities.IsHelpKey(args.KeyEvent))
            {
                TuiScreenUtilities.ShowContextHelp(TuiScreen.AddItem);
                args.Handled = true;
                return;
            }

            switch (args.KeyEvent.Key)
            {
                case Key.Esc:
                    onCancel();
                    args.Handled = true;
                    break;
                case Key.q:
                    onQuit();
                    args.Handled = true;
                    break;
            }
        };

        input.SetFocus();
    }
}
