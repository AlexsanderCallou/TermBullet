using Terminal.Gui;
using TermBullet.Application.Items;
using TermBullet.Core.Items;
using TermBullet.Tui.Navigation;
using TGui = Terminal.Gui.Application;

namespace TermBullet.Tui;

public static class QuickCaptureDialog
{
    public static bool Show(
        TuiScreen screen,
        CreateItemUseCase createItemUseCase,
        CancellationToken cancellationToken = default)
    {
        var saved = false;
        var dialog = new Dialog("Quick Capture", 72, 14);
        var collection = ResolveCollection(screen);

        var hint = new Label("Prefixes: - task, . note, o event. Save uses auto-detection.")
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill(2)
        };

        var collectionLabel = new Label($"collection: {collection.ToString().ToLowerInvariant()}")
        {
            X = 1,
            Y = 3,
            Width = Dim.Fill(2)
        };

        var input = new TextField(string.Empty)
        {
            X = 1,
            Y = 5,
            Width = Dim.Fill(2)
        };

        var saveButton = new Button("Save")
        {
            X = Pos.Center() - 20,
            Y = 8
        };
        var taskButton = new Button("Task")
        {
            X = Pos.Right(saveButton) + 2,
            Y = 8
        };
        var noteButton = new Button("Note")
        {
            X = Pos.Right(taskButton) + 2,
            Y = 8
        };
        var eventButton = new Button("Event")
        {
            X = Pos.Right(noteButton) + 2,
            Y = 8
        };
        var cancelButton = new Button("Cancel")
        {
            X = Pos.Right(eventButton) + 2,
            Y = 8
        };

        void Submit(ItemType? forcedType = null)
        {
            try
            {
                var request = QuickCaptureParser.Parse(input.Text?.ToString() ?? string.Empty, collection, forcedType);
                createItemUseCase.ExecuteAsync(request, cancellationToken).GetAwaiter().GetResult();
                saved = true;
                TGui.RequestStop();
            }
            catch (Exception ex)
            {
                MessageBox.ErrorQuery("Quick Capture", ex.Message, "Close");
            }
        }

        saveButton.Clicked += () => Submit();
        taskButton.Clicked += () => Submit(ItemType.Task);
        noteButton.Clicked += () => Submit(ItemType.Note);
        eventButton.Clicked += () => Submit(ItemType.Event);
        cancelButton.Clicked += () => TGui.RequestStop();

        input.KeyPress += args =>
        {
            if (args.KeyEvent.Key == Key.Enter)
            {
                Submit();
                args.Handled = true;
            }
        };

        dialog.Add(hint, collectionLabel, input, saveButton, taskButton, noteButton, eventButton, cancelButton);
        input.SetFocus();
        TGui.Run(dialog);

        return saved;
    }

    private static ItemCollection ResolveCollection(TuiScreen screen) =>
        screen switch
        {
            TuiScreen.WeeklyPlanning => ItemCollection.Week,
            TuiScreen.BacklogTriage => ItemCollection.Backlog,
            TuiScreen.Review => ItemCollection.Monthly,
            _ => ItemCollection.Today
        };
}
