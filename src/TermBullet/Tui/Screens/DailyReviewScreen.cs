using Terminal.Gui;
using TermBullet.Tui.Navigation;

namespace TermBullet.Tui.Screens;

public static class DailyReviewScreen
{
    private static readonly string[] DecisionLabels =
    [
        "keep today",
        "move to week",
        "move to month",
        "move to backlog",
        "mark done",
        "cancel"
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
        var selectedDecision = DailyReviewDecision.KeepToday;
        onSelectedItemChanged(selectedItem);

        var topBar = new Label(" TermBullet - Daily Review")
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill()
        };
        var footer = new Label(" Enter apply  o open  k keep  w/m/b move  x done  z cancel  Tab/1-3 focus  ? help  Esc back  q quit")
        {
            X = 0,
            Y = Pos.AnchorEnd(1),
            Width = Dim.Fill()
        };

        var tasksPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(1, "Stale Today Tasks", navigation, 0))
        {
            X = 0,
            Y = 1,
            Width = Dim.Percent(52),
            Height = Dim.Fill(8)
        };
        var taskList = new ListView(TuiScreenUtilities.SanitizeListItems(BuildRows(rows)))
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        tasksPanel.Add(taskList);

        var decisionPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(2, "Decision", navigation, 1))
        {
            X = Pos.Right(tasksPanel),
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill(8)
        };
        var decisionList = new ListView(TuiScreenUtilities.SanitizeListItems(BuildDecisionRows(selectedDecision)))
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        decisionPanel.Add(decisionList);

        var detailsPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(3, "Details", navigation, 2))
        {
            X = 0,
            Y = Pos.Bottom(tasksPanel),
            Width = Dim.Fill(),
            Height = 7
        };
        var detailsList = new ListView(TuiScreenUtilities.SanitizeListItems(BuildDetails(selectedItem)))
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        detailsPanel.Add(detailsList);

        var panels = new[] { tasksPanel, decisionPanel, detailsPanel };
        var panelTitles = new[] { "Stale Today Tasks", "Decision", "Details" };
        var focusTargets = new View[] { taskList, decisionList, detailsList };

        root.Add(topBar, tasksPanel, decisionPanel, detailsPanel, footer);
        TuiScreenUtilities.UpdatePanelTitles(panels, panelTitles, navigation);
        TuiScreenUtilities.FocusCurrentPanel(focusTargets, navigation);

        taskList.SelectedItemChanged += _ =>
        {
            selectedIndex = taskList.SelectedItem;
            selectedItem = selectedIndex >= 0 && selectedIndex < rows.Count ? rows[selectedIndex] : null;
            TuiScreenUtilities.RefreshListView(detailsList, BuildDetails(selectedItem));
            onSelectedItemChanged(selectedItem);
        };

        decisionList.SelectedItemChanged += _ =>
        {
            selectedDecision = FromDecisionIndex(decisionList.SelectedItem);
            TuiScreenUtilities.RefreshListView(decisionList, BuildDecisionRows(selectedDecision));
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
                    onApplyDecision(selectedItem, selectedDecision);
                    return true;
                case Key o when o == (Key)'o':
                    onOpenDetail(selectedItem);
                    return true;
                case Key k when k == (Key)'k':
                    onApplyDecision(selectedItem, DailyReviewDecision.KeepToday);
                    return true;
                case Key w when w == (Key)'w':
                    onApplyDecision(selectedItem, DailyReviewDecision.MoveToWeek);
                    return true;
                case Key m when m == (Key)'m':
                    onApplyDecision(selectedItem, DailyReviewDecision.MoveToMonth);
                    return true;
                case Key b when b == (Key)'b':
                    onApplyDecision(selectedItem, DailyReviewDecision.MoveToBacklog);
                    return true;
                case Key x when x == (Key)'x':
                    onApplyDecision(selectedItem, DailyReviewDecision.MarkDone);
                    return true;
                case Key z when z == (Key)'z':
                    onApplyDecision(selectedItem, DailyReviewDecision.Cancel);
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

    private static string[] BuildDecisionRows(DailyReviewDecision selectedDecision) =>
        DecisionLabels
            .Select((label, index) => $"{(index == (int)selectedDecision ? "(x)" : "( )")} {label}")
            .ToArray();

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
            $"tag: {row.Item.Tag}",
            $"priority: {row.Item.Priority}",
            row.Item.Content
        ];
    }

    private static DailyReviewDecision FromDecisionIndex(int selectedIndex) =>
        selectedIndex >= 0 && selectedIndex <= (int)DailyReviewDecision.Cancel
            ? (DailyReviewDecision)selectedIndex
            : DailyReviewDecision.KeepToday;
}
