using Terminal.Gui;
using TermBullet.Application.Items;
using TermBullet.Core.Items;

namespace TermBullet.Tui.Screens;

public static class AddItemScreen
{
    private enum FocusArea
    {
        Timing,
        PlannedFor,
        Content,
        Description,
        Tags
    }

    public static void Build(
        View root,
        TuiAddItemViewModel viewModel,
        Action<CreateItemRequest> onSubmit,
        Action onCancel,
        Action onQuit)
    {
        var draft = new AddItemFormDraft { Type = viewModel.Type };
        var focusArea = FocusArea.Content;
        var selectedTiming = AddItemTimingChoice.Today;
        var syncingSelection = false;
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

        var footer = new Label(" Enter add  Tab focus  CursorUp/CursorDown move  Esc cancel  ? help  q quit")
        {
            X = 0,
            Y = Pos.AnchorEnd(1),
            Width = Dim.Fill()
        };

        var contentPanel = new FrameView(isTask ? "Content" : "Title")
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

        var planningPanel = new FrameView(isTask ? "Timing" : isEvent ? "Scheduled for" : "Planning")
        {
            X = 0,
            Y = Pos.Bottom(contentPanel),
            Width = Dim.Fill(),
            Height = isTask ? 7 : 4,
            Visible = isTask || isEvent
        };
        var timingGroup = new RadioGroup(
            ["Today        planned_for: today", "Future date  planned_for: later", "Backlog      planned_for: -"],
            0)
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill(2),
            Visible = isTask
        };
        var plannedLabel = new Label(isEvent ? "Scheduled for:" : "Planned for:")
        {
            X = 1,
            Y = isTask ? 4 : 1,
            Visible = isEvent
        };
        var plannedField = new TextField(DateOnly.FromDateTime(DateTime.Today.AddDays(1)).ToString("yyyy-MM-dd"))
        {
            X = Pos.Right(plannedLabel) + 1,
            Y = isTask ? 4 : 1,
            Width = 12,
            Visible = isEvent
        };
        var plannedHint = new Label("yyyy-mm-dd")
        {
            X = Pos.Right(plannedField) + 1,
            Y = isTask ? 4 : 1,
            Width = 12,
            Visible = isEvent
        };
        planningPanel.Add(timingGroup, plannedLabel, plannedField, plannedHint);

        var detailsPanel = new FrameView("Details")
        {
            X = 0,
            Y = isTask || isEvent ? Pos.Bottom(planningPanel) : Pos.Bottom(contentPanel),
            Width = Dim.Percent(58),
            Height = 9
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
        var tagsLabel = new Label("Tags:")
        {
            X = 1,
            Y = 6
        };
        var tagsField = new TextField(string.Empty)
        {
            X = Pos.Right(tagsLabel) + 1,
            Y = 6,
            Width = Dim.Fill(2)
        };
        detailsPanel.Add(descriptionLabel, descriptionField, tagsLabel, tagsField);

        var examplesPanel = new FrameView("Examples")
        {
            X = Pos.Right(detailsPanel),
            Y = detailsPanel.Y,
            Width = Dim.Fill(),
            Height = 9
        };
        AddExampleLines(examplesPanel, viewModel.Examples);

        var statusLabel = new Label(viewModel.Error is null ? "Status: ready to add" : $"Status: {viewModel.Error}")
        {
            X = 0,
            Y = Pos.Bottom(detailsPanel),
            Width = Dim.Fill()
        };

        root.Add(topBar, contentPanel);
        if (isTask || isEvent)
        {
            root.Add(planningPanel);
        }

        root.Add(detailsPanel, examplesPanel, statusLabel, footer);

        void SetTiming(AddItemTimingChoice timing)
        {
            selectedTiming = timing;
            var selectedIndex = timing switch
            {
                AddItemTimingChoice.FutureDate => 1,
                AddItemTimingChoice.Backlog => 2,
                _ => 0
            };

            if (timingGroup.SelectedItem != selectedIndex)
            {
                syncingSelection = true;
                try
                {
                    timingGroup.SelectedItem = selectedIndex;
                }
                finally
                {
                    syncingSelection = false;
                }
            }

            var showPlannedFor = isEvent || timing == AddItemTimingChoice.FutureDate;
            plannedLabel.Visible = showPlannedFor;
            plannedField.Visible = showPlannedFor;
            plannedHint.Visible = showPlannedFor;
            if (!showPlannedFor && focusArea == FocusArea.PlannedFor)
            {
                SetFocusArea(FocusArea.Content);
            }

            UpdateStatus();
        }

        void SyncDraftFromControls()
        {
            draft.Type = viewModel.Type;
            draft.Timing = selectedTiming;
            draft.Content = contentField.Text?.ToString() ?? string.Empty;
            draft.Description = descriptionField.Text?.ToString() ?? string.Empty;
            draft.TagsText = tagsField.Text?.ToString() ?? string.Empty;
            draft.PlannedForText = plannedField.Text?.ToString() ?? string.Empty;
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
                case FocusArea.PlannedFor:
                    plannedField.SetFocus();
                    break;
                case FocusArea.Content:
                    contentField.SetFocus();
                    break;
                case FocusArea.Description:
                    descriptionField.SetFocus();
                    break;
                case FocusArea.Tags:
                    tagsField.SetFocus();
                    break;
            }
        }

        FocusArea[] GetFocusOrder()
        {
            if (isTask)
            {
                return selectedTiming == AddItemTimingChoice.FutureDate
                    ? [FocusArea.Content, FocusArea.Timing, FocusArea.PlannedFor, FocusArea.Description, FocusArea.Tags]
                    : [FocusArea.Content, FocusArea.Timing, FocusArea.Description, FocusArea.Tags];
            }

            return isEvent
                ? [FocusArea.Content, FocusArea.PlannedFor, FocusArea.Description, FocusArea.Tags]
                : [FocusArea.Content, FocusArea.Description, FocusArea.Tags];
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
                1 => AddItemTimingChoice.FutureDate,
                2 => AddItemTimingChoice.Backlog,
                _ => AddItemTimingChoice.Today
            });
        };

        root.KeyPress += args =>
        {
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
                    timingGroup.SelectedItem = timingGroup.SelectedItem <= 0 ? 2 : timingGroup.SelectedItem - 1;
                    args.Handled = true;
                    break;
                case Key.CursorDown when isTask && focusArea == FocusArea.Timing:
                    timingGroup.SelectedItem = timingGroup.SelectedItem >= 2 ? 0 : timingGroup.SelectedItem + 1;
                    args.Handled = true;
                    break;
                case Key.Space when isTask && focusArea == FocusArea.Timing:
                    timingGroup.SelectedItem = timingGroup.SelectedItem >= 2 ? 0 : timingGroup.SelectedItem + 1;
                    args.Handled = true;
                    break;
                case Key.Enter:
                    Submit();
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

        SetTiming(isEvent ? AddItemTimingChoice.FutureDate : AddItemTimingChoice.Today);
        SetFocusArea(FocusArea.Content);
        UpdateStatus();
    }

    private static void AddExampleLines(FrameView panel, IReadOnlyList<string> examples)
    {
        var header = new Label("Examples:")
        {
            X = 1,
            Y = 1
        };
        panel.Add(header);

        for (var index = 0; index < examples.Count; index++)
        {
            panel.Add(new Label($"  {examples[index]}")
            {
                X = 1,
                Y = index + 2,
                Width = Dim.Fill(2)
            });
        }
    }
}
