using Terminal.Gui;
using TermBullet.Application.Items;
using TermBullet.Domain.Items;

namespace TermBullet.Tui.Screens;

public static class AddItemScreen
{
    private enum FocusArea
    {
        Timing,
        Priority,
        ScheduledAt,
        Content,
        Description,
        Tags,
        Save,
        Cancel
    }

    public static void Build(
        View root,
        TuiAddItemViewModel viewModel,
        IReadOnlyCollection<string> availableTags,
        string? initialTag,
        Action<CreateItemRequest> onSubmit,
        Action onCancel,
        Action onQuit)
    {
        var draft = new AddItemFormDraft
        {
            Type = viewModel.Type,
            SelectedTag = string.IsNullOrWhiteSpace(initialTag) ? Item.DefaultTag : initialTag
        };
        var focusArea = FocusArea.Content;
        var selectedTiming = AddItemTimingChoice.Today;
        var selectedPriority = Priority.None;
        var syncingSelection = false;
        var syncingPrioritySelection = false;
        var isTask = viewModel.Type == ItemType.Task;
        var isEvent = viewModel.Type == ItemType.Event;
        var title = viewModel.Type switch
        {
            ItemType.Note => "TermBullet - Add Note",
            ItemType.Event => "TermBullet - Add Event",
            _ => "TermBullet - Add Task"
        };

        var topBar = new Label($" {title}")
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill()
        };

        var footer = new Label(" Enter activate  Tab focus  Arrows move  Space cycle  Esc cancel  ? help")
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
        var contentLabel = new Label(isTask ? "Task:" : "Title:")
        {
            X = 1,
            Y = 1
        };
        var contentField = new TextField(string.Empty)
        {
            X = Pos.Right(contentLabel) + 1,
            Y = 1,
            Width = Dim.Fill(2)
        };
        contentPanel.Add(contentLabel, contentField);

        var planningPanel = new FrameView(isTask ? "2 Timing" : isEvent ? "2 Scheduled for" : "2 Planning")
        {
            X = 0,
            Y = Pos.Bottom(contentPanel),
            Width = Dim.Fill(),
            Height = isTask ? 7 : 4,
            Visible = isTask || isEvent
        };
        var timingGroup = new ListView(TuiScreenUtilities.SanitizeListItems(BuildTimingRows(selectedTiming)))
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill(2),
            Height = 4,
            Visible = isTask
        };
        timingGroup.SelectedItem = 0;
        var scheduledLabel = new Label("Scheduled for:")
        {
            X = 1,
            Y = isTask ? 4 : 1,
            Visible = isEvent
        };
        var scheduledField = new TextField(DateOnly.FromDateTime(DateTime.Today.AddDays(1)).ToString("yyyy-MM-dd"))
        {
            X = Pos.Right(scheduledLabel) + 1,
            Y = isTask ? 4 : 1,
            Width = 12,
            Visible = isEvent
        };
        var scheduledHint = new Label("yyyy-mm-dd")
        {
            X = Pos.Right(scheduledField) + 1,
            Y = isTask ? 4 : 1,
            Width = 12,
            Visible = isEvent
        };
        planningPanel.Add(timingGroup, scheduledLabel, scheduledField, scheduledHint);

        var priorityPanel = new FrameView("3 Priority")
        {
            X = 0,
            Y = Pos.Bottom(planningPanel),
            Width = Dim.Fill(),
            Height = 7,
            Visible = isTask
        };
        var priorityGroup = new ListView(TuiScreenUtilities.SanitizeListItems(BuildPriorityRows(selectedPriority)))
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill(2),
            Height = 4,
            Visible = isTask
        };
        priorityGroup.SelectedItem = 0;
        priorityPanel.Add(priorityGroup);

        var detailsPanel = new FrameView(isTask ? "4 Details" : isEvent ? "3 Details" : "2 Details")
        {
            X = 0,
            Y = isTask ? Pos.Bottom(priorityPanel) : isEvent ? Pos.Bottom(planningPanel) : Pos.Bottom(contentPanel),
            Width = Dim.Fill(),
            Height = 11
        };
        var descriptionLabel = new Label("Description:")
        {
            X = 1,
            Y = 1
        };
        var descriptionField = new TextView()
        {
            X = 1,
            Y = 2,
            Width = Dim.Fill(2),
            Height = 3
        };
        var tagsLabel = new Label("Tag:")
        {
            X = 1,
            Y = 6
        };
        var tagSelection = new TagSelectionList(availableTags, draft.SelectedTag);
        var tagsList = tagSelection.View;
        tagsList.X = Pos.Right(tagsLabel) + 1;
        tagsList.Y = 6;
        tagsList.Width = Dim.Fill(2);
        tagsList.Height = 3;
        detailsPanel.Add(descriptionLabel, descriptionField, tagsLabel, tagsList);
        var tagsHint = new Label("Space select")
        {
            X = Pos.Right(tagsLabel) + 1,
            Y = 9,
            Width = Dim.Fill(2)
        };
        detailsPanel.Add(tagsHint);

        var statusLabel = new Label(viewModel.Error is null ? "Status: ready to add" : $"Status: {viewModel.Error}")
        {
            X = 0,
            Y = Pos.Bottom(detailsPanel),
            Width = Dim.Fill()
        };
        var saveButton = new Button("Save")
        {
            X = 0,
            Y = Pos.Bottom(statusLabel) + 1
        };
        var cancelButton = new Button("Cancel")
        {
            X = Pos.Right(saveButton) + 2,
            Y = Pos.Bottom(statusLabel) + 1
        };

        root.Add(topBar, contentPanel);
        if (isTask || isEvent)
        {
            root.Add(planningPanel);
        }

        if (isTask)
        {
            root.Add(priorityPanel);
        }

        root.Add(detailsPanel, statusLabel, saveButton, cancelButton, footer);

        void SetTiming(AddItemTimingChoice timing)
        {
            selectedTiming = timing;
            var selectedIndex = timing switch
            {
            AddItemTimingChoice.Week => 1,
            AddItemTimingChoice.Month => 2,
            AddItemTimingChoice.Backlog => 3,
                _ => 0
            };

            syncingSelection = true;
            try
            {
                TuiScreenUtilities.RefreshListView(timingGroup, BuildTimingRows(selectedTiming));
                timingGroup.SelectedItem = selectedIndex;
            }
            finally
            {
                syncingSelection = false;
            }

            var showSchedule = isEvent;
            scheduledLabel.Visible = showSchedule;
            scheduledField.Visible = showSchedule;
            scheduledHint.Visible = showSchedule;
            if (!showSchedule && focusArea == FocusArea.ScheduledAt)
            {
                SetFocusArea(FocusArea.Content);
            }

            UpdateStatus();
        }

        void SetPriority(Priority priority)
        {
            selectedPriority = priority;
            var selectedIndex = priority switch
            {
                Priority.Low => 1,
                Priority.Medium => 2,
                Priority.High => 3,
                _ => 0
            };

            syncingPrioritySelection = true;
            try
            {
                TuiScreenUtilities.RefreshListView(priorityGroup, BuildPriorityRows(selectedPriority));
                priorityGroup.SelectedItem = selectedIndex;
            }
            finally
            {
                syncingPrioritySelection = false;
            }

            UpdateStatus();
        }

        void SyncDraftFromControls()
        {
            draft.Type = viewModel.Type;
            draft.Timing = selectedTiming;
            draft.Priority = isTask ? selectedPriority : Priority.None;
            draft.Content = contentField.Text?.ToString() ?? string.Empty;
            draft.Description = descriptionField.Text?.ToString() ?? string.Empty;
            draft.SelectedTag = tagSelection.SelectedTag;
            draft.ScheduledAtText = scheduledField.Text?.ToString() ?? string.Empty;
        }

        void UpdateStatus()
        {
            SyncDraftFromControls();

            try
            {
                var summary = draft.BuildPreviewLines();
                statusLabel.Text = $"Status: {string.Join(" | ", summary)}";
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                statusLabel.Text = $"Status: {ex.Message}";
            }
        }

        void SetFocusArea(FocusArea area)
        {
            focusArea = area;
            switch (area)
            {
                case FocusArea.Timing:
                    timingGroup.SetFocus();
                    break;
                case FocusArea.Priority:
                    priorityGroup.SetFocus();
                    break;
                case FocusArea.ScheduledAt:
                    scheduledField.SetFocus();
                    break;
                case FocusArea.Content:
                    contentField.SetFocus();
                    break;
                case FocusArea.Description:
                    descriptionField.SetFocus();
                    break;
                case FocusArea.Tags:
                    tagsList.SetFocus();
                    break;
                case FocusArea.Save:
                    saveButton.SetFocus();
                    break;
                case FocusArea.Cancel:
                    cancelButton.SetFocus();
                    break;
            }
        }

        FocusArea[] GetFocusOrder()
        {
            if (isTask)
            {
                return [FocusArea.Content, FocusArea.Timing, FocusArea.Priority, FocusArea.Description, FocusArea.Tags, FocusArea.Save, FocusArea.Cancel];
            }

            return isEvent
                ? [FocusArea.Content, FocusArea.ScheduledAt, FocusArea.Description, FocusArea.Tags, FocusArea.Save, FocusArea.Cancel]
                : [FocusArea.Content, FocusArea.Description, FocusArea.Tags, FocusArea.Save, FocusArea.Cancel];
        }

        void MoveFocus(int delta)
        {
            var order = GetFocusOrder();
            var index = Array.IndexOf(order, focusArea);
            if (index < 0)
            {
                index = 0;
            }

            index += delta;
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

        void Submit()
        {
            try
            {
                SyncDraftFromControls();
                onSubmit(draft.BuildRequest());
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                statusLabel.Text = $"Status: {ex.Message}";
            }
        }

        timingGroup.SelectedItemChanged += _ =>
        {
            if (syncingSelection)
            {
                return;
            }

            SetTiming(timingGroup.SelectedItem switch
            {
                1 => AddItemTimingChoice.Week,
                2 => AddItemTimingChoice.Month,
                3 => AddItemTimingChoice.Backlog,
                _ => AddItemTimingChoice.Today
            });
        };

        priorityGroup.SelectedItemChanged += _ =>
        {
            if (syncingPrioritySelection)
            {
                return;
            }

            SetPriority(priorityGroup.SelectedItem switch
            {
                1 => Priority.Low,
                2 => Priority.Medium,
                3 => Priority.High,
                _ => Priority.None
            });
        };

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
            };
        }

        AttachTextNavigation(contentField);
        AttachTextNavigation(scheduledField);
        AttachTextNavigation(descriptionField);
        tagsList.KeyPress += args =>
        {
            switch (args.KeyEvent.Key)
            {
                case Key.Space:
                    tagSelection.ToggleSelected();
                    UpdateStatus();
                    args.Handled = true;
                    break;
                case Key.Tab:
                    MoveFocus(1);
                    args.Handled = true;
                    break;
                case Key.BackTab:
                    MoveFocus(-1);
                    args.Handled = true;
                    break;
            }
        };

        root.KeyPress += args =>
        {
            if (TuiScreenUtilities.ShouldLetTextInputHandle(
                args.KeyEvent,
                contentField,
                scheduledField,
                descriptionField))
            {
                return;
            }

            if (TuiScreenUtilities.IsHelpKey(args.KeyEvent))
            {
                TuiScreenUtilities.ShowAddItemHelp();
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
                case Key.CursorUp when isTask && focusArea == FocusArea.Timing:
                    timingGroup.SelectedItem = timingGroup.SelectedItem <= 0 ? 3 : timingGroup.SelectedItem - 1;
                    args.Handled = true;
                    break;
                case Key.CursorDown when isTask && focusArea == FocusArea.Timing:
                    timingGroup.SelectedItem = timingGroup.SelectedItem >= 3 ? 0 : timingGroup.SelectedItem + 1;
                    args.Handled = true;
                    break;
                case Key.Space when isTask && focusArea == FocusArea.Timing:
                    timingGroup.SelectedItem = timingGroup.SelectedItem >= 3 ? 0 : timingGroup.SelectedItem + 1;
                    args.Handled = true;
                    break;
                case Key.CursorUp when isTask && focusArea == FocusArea.Priority:
                    priorityGroup.SelectedItem = priorityGroup.SelectedItem <= 0 ? 3 : priorityGroup.SelectedItem - 1;
                    args.Handled = true;
                    break;
                case Key.CursorDown when isTask && focusArea == FocusArea.Priority:
                    priorityGroup.SelectedItem = priorityGroup.SelectedItem >= 3 ? 0 : priorityGroup.SelectedItem + 1;
                    args.Handled = true;
                    break;
                case Key.Space when isTask && focusArea == FocusArea.Priority:
                    priorityGroup.SelectedItem = priorityGroup.SelectedItem >= 3 ? 0 : priorityGroup.SelectedItem + 1;
                    args.Handled = true;
                    break;
                case Key.Space when focusArea == FocusArea.Tags:
                    tagSelection.ToggleSelected();
                    UpdateStatus();
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

        SetTiming(AddItemTimingChoice.Today);
        SetPriority(Priority.None);
        SetFocusArea(FocusArea.Content);
        UpdateStatus();

    }

    private static string[] BuildTimingRows(AddItemTimingChoice selectedTiming) =>
    [
        TuiAsciiControls.RadioLine(selectedTiming == AddItemTimingChoice.Today, "Today"),
        TuiAsciiControls.RadioLine(selectedTiming == AddItemTimingChoice.Week, "Week"),
        TuiAsciiControls.RadioLine(selectedTiming == AddItemTimingChoice.Month, "Month"),
        TuiAsciiControls.RadioLine(selectedTiming == AddItemTimingChoice.Backlog, "Backlog")
    ];

    private static string[] BuildPriorityRows(Priority selectedPriority) =>
    [
        TuiAsciiControls.RadioLine(selectedPriority == Priority.None, "None"),
        TuiAsciiControls.RadioLine(selectedPriority == Priority.Low, "Low"),
        TuiAsciiControls.RadioLine(selectedPriority == Priority.Medium, "Medium"),
        TuiAsciiControls.RadioLine(selectedPriority == Priority.High, "High")
    ];
}
