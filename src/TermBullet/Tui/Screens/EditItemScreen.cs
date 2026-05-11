using Terminal.Gui;
using TermBullet.Application.Items;
using TermBullet.Domain.Items;

namespace TermBullet.Tui.Screens;

public static class EditItemScreen
{
    private enum FocusArea
    {
        Content,
        Collection,
        Priority,
        ScheduledAt,
        Description,
        Tags,
        Save,
        Cancel
    }

    public static void Build(
        View root,
        EditItemFormDraft draft,
        string? error,
        Action<EditItemRequest> onSubmit,
        Action onCancel,
        Action onQuit)
    {
        var isTask = draft.Type == ItemType.Task;
        var isEvent = draft.Type == ItemType.Event;
        var focusArea = FocusArea.Content;

        var title = draft.Type switch
        {
            ItemType.Note => $"TermBullet - Edit Note {draft.PublicRef}",
            ItemType.Event => $"TermBullet - Edit Event {draft.PublicRef}",
            _ => $"TermBullet - Edit Task {draft.PublicRef}"
        };

        var topBar = new Label($" {title}") { X = 0, Y = 0, Width = Dim.Fill() };
        var footer = new Label(" Enter activate  Tab/1-5 focus  Arrows move  Space cycle  Esc cancel  ? help  q quit")
        {
            X = 0,
            Y = Pos.AnchorEnd(1),
            Width = Dim.Fill()
        };

        var contentPanel = new FrameView(isTask ? "1 Content" : "1 Title")
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill(),
            Height = 5
        };
        var contentField = new TextField(draft.Content)
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill(2)
        };
        contentPanel.Add(contentField);

        var planningPanel = new FrameView(isTask ? "2 Collection" : "2 Scheduled for")
        {
            X = 0,
            Y = Pos.Bottom(contentPanel),
            Width = Dim.Fill(),
            Height = isTask ? 7 : 4,
            Visible = isTask || isEvent
        };
        var collectionGroup = new RadioGroup(["Today", "Week", "Month", "Backlog"], CollectionIndex(draft.Collection))
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill(2),
            Height = 4,
            Visible = isTask
        };
        var scheduledField = new TextField(draft.ScheduledAtText)
        {
            X = 1,
            Y = 1,
            Width = 12,
            Visible = isEvent
        };
        var scheduledHint = new Label("yyyy-mm-dd")
        {
            X = Pos.Right(scheduledField) + 1,
            Y = 1,
            Visible = isEvent
        };
        planningPanel.Add(collectionGroup, scheduledField, scheduledHint);

        var priorityPanel = new FrameView("3 Priority")
        {
            X = 0,
            Y = Pos.Bottom(planningPanel),
            Width = Dim.Fill(),
            Height = 6,
            Visible = isTask
        };
        var priorityGroup = new RadioGroup(["None", "Low", "Medium", "High"], PriorityIndex(draft.Priority))
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill(2),
            Height = 4,
            Visible = isTask
        };
        priorityPanel.Add(priorityGroup);

        var detailsPanel = new FrameView(isTask ? "4 Details" : isEvent ? "3 Details" : "2 Details")
        {
            X = 0,
            Y = isTask ? Pos.Bottom(priorityPanel) : isEvent ? Pos.Bottom(planningPanel) : Pos.Bottom(contentPanel),
            Width = Dim.Fill(),
            Height = 9
        };
        var descriptionLabel = new Label("Description:") { X = 1, Y = 1 };
        var descriptionField = new TextView
        {
            X = 1,
            Y = 2,
            Width = Dim.Fill(2),
            Height = 3,
            Text = draft.Description
        };
        var tagsLabel = new Label("Tags:") { X = 1, Y = 6 };
        var tagsField = new TextField(draft.TagsText)
        {
            X = Pos.Right(tagsLabel) + 1,
            Y = 6,
            Width = Dim.Fill(2)
        };
        detailsPanel.Add(descriptionLabel, descriptionField, tagsLabel, tagsField);

        var statusLabel = new Label(error is null ? "Status: ready to edit" : $"Status: {error}")
        {
            X = 0,
            Y = Pos.Bottom(detailsPanel),
            Width = Dim.Fill()
        };
        var saveButton = new Button("Save") { X = 0, Y = Pos.Bottom(statusLabel) + 1 };
        var cancelButton = new Button("Cancel") { X = Pos.Right(saveButton) + 2, Y = Pos.Bottom(statusLabel) + 1 };

        root.Add(topBar, contentPanel);
        if (isTask || isEvent) root.Add(planningPanel);
        if (isTask) root.Add(priorityPanel);
        root.Add(detailsPanel, statusLabel, saveButton, cancelButton, footer);

        void SyncDraft()
        {
            draft.Content = contentField.Text?.ToString() ?? string.Empty;
            draft.Description = descriptionField.Text?.ToString() ?? string.Empty;
            draft.TagsText = tagsField.Text?.ToString() ?? string.Empty;
            draft.Collection = CollectionFromIndex(collectionGroup.SelectedItem);
            draft.Priority = PriorityFromIndex(priorityGroup.SelectedItem);
            draft.ScheduledAtText = scheduledField.Text?.ToString() ?? string.Empty;
        }

        void UpdateStatus()
        {
            SyncDraft();
            try
            {
                statusLabel.Text = $"Status: {string.Join(" | ", draft.BuildPreviewLines())}";
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                statusLabel.Text = $"Status: {ex.Message}";
            }
        }

        void SetFocus(FocusArea area)
        {
            focusArea = area;
            switch (area)
            {
                case FocusArea.Collection:
                    collectionGroup.SetFocus();
                    break;
                case FocusArea.Priority:
                    priorityGroup.SetFocus();
                    break;
                case FocusArea.ScheduledAt:
                    scheduledField.SetFocus();
                    break;
                case FocusArea.Description:
                    descriptionField.SetFocus();
                    break;
                case FocusArea.Tags:
                    tagsField.SetFocus();
                    break;
                case FocusArea.Save:
                    saveButton.SetFocus();
                    break;
                case FocusArea.Cancel:
                    cancelButton.SetFocus();
                    break;
                default:
                    contentField.SetFocus();
                    break;
            }
        }

        FocusArea[] FocusOrder() =>
            isTask
                ? [FocusArea.Content, FocusArea.Collection, FocusArea.Priority, FocusArea.Description, FocusArea.Tags, FocusArea.Save, FocusArea.Cancel]
                : isEvent
                    ? [FocusArea.Content, FocusArea.ScheduledAt, FocusArea.Description, FocusArea.Tags, FocusArea.Save, FocusArea.Cancel]
                    : [FocusArea.Content, FocusArea.Description, FocusArea.Tags, FocusArea.Save, FocusArea.Cancel];

        void MoveFocus(int delta)
        {
            var order = FocusOrder();
            var index = Array.IndexOf(order, focusArea);
            index = index < 0 ? 0 : index + delta;
            if (index < 0) index = order.Length - 1;
            if (index >= order.Length) index = 0;
            SetFocus(order[index]);
        }

        void Submit()
        {
            try
            {
                SyncDraft();
                onSubmit(draft.BuildRequest());
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                statusLabel.Text = $"Status: {ex.Message}";
            }
        }

        collectionGroup.SelectedItemChanged += _ => UpdateStatus();
        priorityGroup.SelectedItemChanged += _ => UpdateStatus();
        saveButton.Clicked += Submit;
        cancelButton.Clicked += onCancel;

        root.KeyPress += args =>
        {
            if (TuiScreenUtilities.IsHelpKey(args.KeyEvent))
            {
                TuiScreenUtilities.ShowContextHelp(Tui.Navigation.TuiScreen.ItemDetail);
                args.Handled = true;
                return;
            }

            var digit = TuiScreenUtilities.GetDigit(args.KeyEvent);
            if (digit is not null)
            {
                var order = FocusOrder();
                if (digit.Value >= 1 && digit.Value <= order.Length)
                {
                    SetFocus(order[digit.Value - 1]);
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
                case Key.CursorUp when isTask && focusArea == FocusArea.Collection:
                    collectionGroup.SelectedItem = collectionGroup.SelectedItem <= 0 ? 3 : collectionGroup.SelectedItem - 1;
                    args.Handled = true;
                    break;
                case Key.CursorDown when isTask && focusArea == FocusArea.Collection:
                case Key.Space when isTask && focusArea == FocusArea.Collection:
                    collectionGroup.SelectedItem = collectionGroup.SelectedItem >= 3 ? 0 : collectionGroup.SelectedItem + 1;
                    args.Handled = true;
                    break;
                case Key.CursorUp when isTask && focusArea == FocusArea.Priority:
                    priorityGroup.SelectedItem = priorityGroup.SelectedItem <= 0 ? 3 : priorityGroup.SelectedItem - 1;
                    args.Handled = true;
                    break;
                case Key.CursorDown when isTask && focusArea == FocusArea.Priority:
                case Key.Space when isTask && focusArea == FocusArea.Priority:
                    priorityGroup.SelectedItem = priorityGroup.SelectedItem >= 3 ? 0 : priorityGroup.SelectedItem + 1;
                    args.Handled = true;
                    break;
                case Key.Enter when focusArea == FocusArea.Save:
                    Submit();
                    args.Handled = true;
                    break;
                case Key.Enter when focusArea == FocusArea.Cancel:
                    onCancel();
                    args.Handled = true;
                    break;
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

        SetFocus(FocusArea.Content);
        UpdateStatus();
    }

    private static int CollectionIndex(ItemCollection collection) =>
        collection switch
        {
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

    private static int PriorityIndex(Priority priority) =>
        priority switch
        {
            Priority.Low => 1,
            Priority.Medium => 2,
            Priority.High => 3,
            _ => 0
        };

    private static Priority PriorityFromIndex(int index) =>
        index switch
        {
            1 => Priority.Low,
            2 => Priority.Medium,
            3 => Priority.High,
            _ => Priority.None
        };
}
