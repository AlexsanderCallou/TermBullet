using Terminal.Gui;
using TermBullet.Tui.Navigation;
using TGui = Terminal.Gui.Application;

namespace TermBullet.Tui.Screens;

public static class DailyFocusScreen
{
    public static void Build(
        Toplevel top,
        DailyFocusViewModel viewModel,
        TuiNavigationState navigation,
        Action onBack,
        Action? onQuickCapture = null)
    {
        var date = DateTime.Today.ToString("yyyy-MM-dd");
        var topBar = new Label($" TermBullet \u2500 Today {date} \u2500 data:local \u2500 ai:off \u2500 sync:idle \u2500 mode:normal")
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill()
        };

        var footer = new Label(" / filter  c capture  e edit  x done  > migrate  Enter detail  Tab focus  ? help  q quit")
        {
            X = 0,
            Y = Pos.AnchorEnd(1),
            Width = Dim.Fill()
        };

        var upperHeight = Dim.Percent(55);

        // Panel 1: Sections
        var sectionsPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(1, "Sections", navigation, 0))
        {
            X = 0,
            Y = 1,
            Width = Dim.Percent(20),
            Height = upperHeight
        };
        var sectionNames = new[] { "> Open", "  In Progress", "  Done", "  Cancelled", "  Migrated" };
        var sectionsList = new ListView(sectionNames)
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };
        sectionsPanel.Add(sectionsList);

        // Panel 2: Daily Log
        var logPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(2, "Daily Log", navigation, 1))
        {
            X = Pos.Right(sectionsPanel),
            Y = 1,
            Width = Dim.Percent(50),
            Height = upperHeight
        };
        var logRows = viewModel.ActiveSectionItems.Count > 0
            ? viewModel.ActiveSectionItems.Select(r => $"{r.Symbol} {r.PublicRef} {r.Content}").ToArray()
            : new[] { "(no items)" };
        var logList = new ListView(logRows)
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };
        logPanel.Add(logList);

        // Panel 3: Details
        var detailsPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(3, "Details", navigation, 2))
        {
            X = Pos.Right(logPanel),
            Y = 1,
            Width = Dim.Fill(),
            Height = upperHeight
        };
        var selected = viewModel.SelectedItem;
        var detailLines = selected is not null
            ? new[]
            {
                selected.PublicRef,
                $"type: {selected.Type}",
                $"status: {selected.Status}",
                $"priority: {selected.Priority}",
                $"collection: {selected.Collection}",
                $"tags: {(selected.Tags.Length > 0 ? string.Join(", ", selected.Tags) : "(none)")}"
            }
            : new[] { "(nothing selected)" };
        var detailList = new ListView(detailLines)
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };
        detailsPanel.Add(detailList);

        var quickCapturePanel = new FrameView(TuiScreenUtilities.GetPanelTitle(4, "Quick Capture", navigation, 3))
        {
            X = 0,
            Y = Pos.Bottom(sectionsPanel),
            Width = Dim.Percent(20),
            Height = Dim.Fill(1)
        };
        quickCapturePanel.Add(new ListView(viewModel.QuickCaptureExamples.ToArray())
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        });

        var shortHistoryPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(5, "Short History", navigation, 4))
        {
            X = Pos.Right(quickCapturePanel),
            Y = Pos.Bottom(logPanel),
            Width = Dim.Percent(50),
            Height = Dim.Fill(1)
        };
        shortHistoryPanel.Add(new ListView(viewModel.ShortHistoryLines.ToArray())
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        });

        var actionsPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(6, "Actions", navigation, 5))
        {
            X = Pos.Right(shortHistoryPanel),
            Y = Pos.Bottom(detailsPanel),
            Width = Dim.Fill(),
            Height = Dim.Fill(1)
        };
        actionsPanel.Add(new ListView(viewModel.ActionHints.ToArray())
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        });

        var panels = new[]
        {
            sectionsPanel, logPanel, detailsPanel, quickCapturePanel, shortHistoryPanel, actionsPanel
        };
        var panelTitles = new[]
        {
            "Sections", "Daily Log", "Details", "Quick Capture", "Short History", "Actions"
        };
        var focusTargets = new View[]
        {
            sectionsList, logList, detailList, quickCapturePanel, shortHistoryPanel, actionsPanel
        };
        TuiScreenUtilities.UpdatePanelTitles(panels, panelTitles, navigation);
        TuiScreenUtilities.FocusCurrentPanel(focusTargets, navigation);

        top.Add(topBar, sectionsPanel, logPanel, detailsPanel, quickCapturePanel, shortHistoryPanel, actionsPanel, footer);

        top.KeyPress += args =>
        {
            switch (args.KeyEvent.Key)
            {
                case Key.Tab:
                    navigation.MoveNextPanel();
                    TuiScreenUtilities.UpdatePanelTitles(panels, panelTitles, navigation);
                    TuiScreenUtilities.FocusCurrentPanel(focusTargets, navigation);
                    args.Handled = true;
                    break;
                case Key.BackTab:
                    navigation.MovePreviousPanel();
                    TuiScreenUtilities.UpdatePanelTitles(panels, panelTitles, navigation);
                    TuiScreenUtilities.FocusCurrentPanel(focusTargets, navigation);
                    args.Handled = true;
                    break;
                case Key x when x == (Key)'?':
                    TuiScreenUtilities.ShowContextHelp(TuiScreen.DailyFocus);
                    args.Handled = true;
                    break;
                case Key x when x == (Key)'c' && onQuickCapture is not null:
                    onQuickCapture();
                    args.Handled = true;
                    break;
                case Key.q:
                    TGui.RequestStop();
                    args.Handled = true;
                    break;
                case Key.Esc:
                    onBack();
                    args.Handled = true;
                    break;
            }
        };

        sectionsList.SelectedItemChanged += _ =>
        {
            var section = sectionsList.SelectedItem switch
            {
                0 => DailyFocusSection.Open,
                1 => DailyFocusSection.InProgress,
                2 => DailyFocusSection.Done,
                3 => DailyFocusSection.Cancelled,
                4 => DailyFocusSection.Migrated,
                _ => DailyFocusSection.Open
            };
            viewModel.ChangeSection(section);
            TuiScreenUtilities.RefreshListView(
                logList,
                viewModel.ActiveSectionItems.Count > 0
                    ? viewModel.ActiveSectionItems.Select(r => $"{r.Symbol} {r.PublicRef} {r.Content}").ToArray()
                    : ["(no items)"]);
            if (viewModel.SelectedItemIndex >= 0)
            {
                logList.SelectedItem = viewModel.SelectedItemIndex;
            }
            TuiScreenUtilities.RefreshListView(detailList, BuildDetailLines(viewModel.SelectedItem));
        };

        logList.SelectedItemChanged += _ =>
        {
            var diff = logList.SelectedItem - viewModel.SelectedItemIndex;
            if (diff > 0)
                for (var i = 0; i < diff; i++) viewModel.SelectNextItem();
            else if (diff < 0)
                for (var i = 0; i < -diff; i++) viewModel.SelectPreviousItem();

            TuiScreenUtilities.RefreshListView(detailList, BuildDetailLines(viewModel.SelectedItem));
        };
    }

    private static string[] BuildDetailLines(ItemDisplayRow? selected) =>
        selected is not null
            ?
            [
                selected.PublicRef,
                $"type: {selected.Type}",
                $"status: {selected.Status}",
                $"priority: {selected.Priority}",
                $"collection: {selected.Collection}",
                $"tags: {(selected.Tags.Length > 0 ? string.Join(", ", selected.Tags) : "(none)")}"
            ]
            : ["(nothing selected)"];
}
