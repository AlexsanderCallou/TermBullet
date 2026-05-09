using Terminal.Gui;
using TermBullet.Application.Items;

namespace TermBullet.Tui.Screens;

public static class QuickTaskScreen
{
    public static void Build(
        View root,
        string? error,
        Action<CreateItemRequest> onSubmit,
        Action onCancel)
    {
        var panel = new FrameView("Quick Task")
        {
            X = Pos.Center() - 32,
            Y = Pos.Center() - 4,
            Width = 64,
            Height = 8
        };
        var taskLabel = new Label("Task:")
        {
            X = 1,
            Y = 1
        };
        var taskField = new TextField(string.Empty)
        {
            X = Pos.Right(taskLabel) + 1,
            Y = 1,
            Width = Dim.Fill(2)
        };
        var plannedLabel = new Label("planned_for: today")
        {
            X = 1,
            Y = 3,
            Width = Dim.Fill(2)
        };
        var statusLabel = new Label(error is null ? "Enter add  Esc cancel" : $"Status: {error}")
        {
            X = 1,
            Y = 5,
            Width = Dim.Fill(2)
        };
        panel.Add(taskLabel, taskField, plannedLabel, statusLabel);
        root.Add(panel);

        void Submit()
        {
            try
            {
                onSubmit(AddItemFormDraft.BuildQuickTaskRequest(taskField.Text?.ToString() ?? string.Empty));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                statusLabel.Text = $"Status: {ex.Message}";
            }
        }

        root.KeyPress += args =>
        {
            switch (args.KeyEvent.Key)
            {
                case Key.Enter:
                    Submit();
                    args.Handled = true;
                    break;
                case Key.Esc:
                    onCancel();
                    args.Handled = true;
                    break;
            }
        };

        taskField.SetFocus();
    }
}
