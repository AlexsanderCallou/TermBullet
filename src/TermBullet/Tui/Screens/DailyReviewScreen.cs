using Terminal.Gui;
using NStack;
using TermBullet.Tui.Navigation;
using TGui = Terminal.Gui.Application;

namespace TermBullet.Tui.Screens;

public static class DailyReviewScreen
{
    private static readonly ustring[] DecisionLabels =
    [
        "Keep today",
        "Move to week",
        "Move to month",
        "Move to backlog",
        "Mark done",
        "Cancel task"
    ];

    public static void Build(
        View root,
        IReadOnlyList<DailyReviewRow> rows,
        TuiNavigationState navigation,
        Action<DailyReviewRow?> onSelectedItemChanged,
        Action<DailyReviewRow?> onOpenDetail,
        Action<DailyReviewRow?, DailyReviewDecision> onApplyDecision,
        Action onBack,
        Action onQuit)
    {
        var selectedIndex = rows.Count > 0 ? 0 : -1;
        var selectedItem = selectedIndex >= 0 ? rows[selectedIndex] : null;
        onSelectedItemChanged(selectedItem);

        var topBar = new Label(" TermBullet - Daily Review")
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill()
        };
        var footer = new Label(" Enter decide  o open  Tab/1-2 focus  ? help  Esc back  q quit")
        {
            X = 0,
            Y = Pos.AnchorEnd(1),
            Width = Dim.Fill()
        };

        var tasksPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(1, "Stale Today Tasks", navigation, 0))
        {
            X = 0,
            Y = 1,
            Width = Dim.Percent(58),
            Height = Dim.Fill(1)
        };
        var taskList = new ListView(TuiScreenUtilities.SanitizeListItems(BuildRows(rows)))
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        tasksPanel.Add(taskList);

        var detailsPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(2, "Details", navigation, 1))
        {
            X = Pos.Right(tasksPanel),
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill(1)
        };
        var detailsList = new ListView(TuiScreenUtilities.SanitizeListItems(BuildDetails(selectedItem)))
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        detailsPanel.Add(detailsList);

        var panels = new[] { tasksPanel, detailsPanel };
        var panelTitles = new[] { "Stale Today Tasks", "Details" };
        var focusTargets = new View[] { taskList, detailsList };

        root.Add(topBar, tasksPanel, detailsPanel, footer);
        TuiScreenUtilities.UpdatePanelTitles(panels, panelTitles, navigation);
        TuiScreenUtilities.FocusCurrentPanel(focusTargets, navigation);

        taskList.SelectedItemChanged += _ =>
        {
            selectedIndex = taskList.SelectedItem;
            selectedItem = selectedIndex >= 0 && selectedIndex < rows.Count ? rows[selectedIndex] : null;
            TuiScreenUtilities.RefreshListView(detailsList, BuildDetails(selectedItem));
            onSelectedItemChanged(selectedItem);
        };

        bool HandleKey(KeyEvent keyEvent, bool includeEnter)
        {
            if (TuiScreenUtilities.IsHelpKey(keyEvent))
            {
                TuiScreenUtilities.ShowContextHelp(TuiScreen.DailyReview);
                return true;
            }

            if (TuiScreenUtilities.TryFocusPanelByNumber(keyEvent, navigation, panels, panelTitles, focusTargets))
            {
                return true;
            }

            switch (keyEvent.Key)
            {
                case Key.Tab:
                    navigation.MoveNextPanel();
                    TuiScreenUtilities.UpdatePanelTitles(panels, panelTitles, navigation);
                    TuiScreenUtilities.FocusCurrentPanel(focusTargets, navigation);
                    return true;
                case Key.BackTab:
                    navigation.MovePreviousPanel();
                    TuiScreenUtilities.UpdatePanelTitles(panels, panelTitles, navigation);
                    TuiScreenUtilities.FocusCurrentPanel(focusTargets, navigation);
                    return true;
                case Key.Enter when includeEnter:
                    ShowDecisionDialog(selectedItem, onApplyDecision);
                    return true;
                case Key o when o == (Key)'o':
                    onOpenDetail(selectedItem);
                    return true;
                case Key.Esc:
                    onBack();
                    return true;
                case Key.q:
                    onQuit();
                    return true;
            }

            return false;
        }

        root.KeyPress += args =>
        {
            if (HandleKey(args.KeyEvent, includeEnter: true))
            {
                args.Handled = true;
            }
        };

        foreach (var target in focusTargets)
        {
            target.KeyPress += args =>
            {
                if (HandleKey(args.KeyEvent, includeEnter: false))
                {
                    args.Handled = true;
                }
            };
        }

        taskList.SetFocus();
    }

    private static string[] BuildRows(IReadOnlyList<DailyReviewRow> rows)
    {
        if (rows.Count == 0)
        {
            return ["(no stale today tasks)"];
        }

        return rows
            .Select(row => $"{row.Item.Symbol} {row.Item.PublicRef} {row.Item.Content} last: {row.LastTodayPlacementDate:yyyy-MM-dd}".Trim())
            .ToArray();
    }

    private static string[] BuildDetails(DailyReviewRow? row)
    {
        if (row is null)
        {
            return ["(nothing selected)"];
        }

        return
        [
            $"ref: {row.Item.PublicRef}",
            $"last today review: {row.LastTodayPlacementDate:yyyy-MM-dd}",
            $"status: {row.Item.Status}",
            $"collection: {row.Item.Collection}",
            $"tag: {row.Item.Tag}",
            $"priority: {row.Item.Priority}",
            " ",
            row.Item.Content
        ];
    }

    private static void ShowDecisionDialog(
        DailyReviewRow? row,
        Action<DailyReviewRow?, DailyReviewDecision> onApplyDecision)
    {
        if (row is null)
        {
            return;
        }

        var selectedDecision = DailyReviewDecision.KeepToday;
        var saveButton = new Button("Save", is_default: true);
        var cancelButton = new Button("Cancel");
        var dialog = new Dialog("Daily Review Decision", 64, 17, saveButton, cancelButton);
        var summary = new Label($"{row.Item.PublicRef} {row.Item.Content}")
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill(2)
        };
        var prompt = new Label("Choose what to do with this stale Today task:")
        {
            X = 1,
            Y = 3,
            Width = Dim.Fill(2)
        };
        var decisionGroup = new RadioGroup(DecisionLabels, 0)
        {
            X = 1,
            Y = 5,
            Width = Dim.Fill(2)
        };

        var apply = false;
        saveButton.Clicked += () =>
        {
            selectedDecision = FromDecisionIndex(decisionGroup.SelectedItem);
            apply = true;
            TGui.RequestStop();
        };
        cancelButton.Clicked += () => TGui.RequestStop();
        decisionGroup.SelectedItemChanged += _ =>
        {
            selectedDecision = FromDecisionIndex(decisionGroup.SelectedItem);
        };
        dialog.KeyPress += args =>
        {
            switch (args.KeyEvent.Key)
            {
                case Key.Space:
                    decisionGroup.SelectedItem = decisionGroup.SelectedItem >= (int)DailyReviewDecision.Cancel
                        ? 0
                        : decisionGroup.SelectedItem + 1;
                    args.Handled = true;
                    break;
                case Key.Enter:
                    selectedDecision = FromDecisionIndex(decisionGroup.SelectedItem);
                    apply = true;
                    TGui.RequestStop();
                    args.Handled = true;
                    break;
                case Key.Esc:
                    TGui.RequestStop();
                    args.Handled = true;
                    break;
            }
        };

        dialog.Add(summary, prompt, decisionGroup);
        decisionGroup.SetFocus();
        TGui.Run(dialog);

        if (apply)
        {
            onApplyDecision(row, selectedDecision);
        }
    }

    private static DailyReviewDecision FromDecisionIndex(int selectedIndex) =>
        selectedIndex >= 0 && selectedIndex <= (int)DailyReviewDecision.Cancel
            ? (DailyReviewDecision)selectedIndex
            : DailyReviewDecision.KeepToday;
}
