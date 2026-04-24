using Terminal.Gui;
using TermBullet.Tui.Navigation;

namespace TermBullet.Tui.Screens;

public static class ReviewScreen
{
    public static void Build(
        View root,
        ReviewViewModel viewModel,
        TuiNavigationState navigation,
        Action onBack,
        Action onQuit,
        Action? onQuickCapture = null)
    {
        var topBar = new Label(" TermBullet \u2500 Review \u2500 data:local \u2500 ai:off \u2500 sync:idle \u2500 mode:normal")
        {
            X = 0, Y = 0, Width = Dim.Fill()
        };

        var footer = new Label(" r generate review  a AI  c add  > migrate pending  Tab focus  ? help  q quit")
        {
            X = 0, Y = Pos.AnchorEnd(1), Width = Dim.Fill()
        };

        var collectionsPanel = BuildPanel(TuiScreenUtilities.GetPanelTitle(1, "Collections", navigation, 0), 0, 1, Dim.Percent(20), Dim.Percent(55), viewModel.Collections);
        var summaryPanel = BuildPanel(TuiScreenUtilities.GetPanelTitle(2, "Summary", navigation, 1), Pos.Right(collectionsPanel), 1, Dim.Percent(40), Dim.Percent(55), viewModel.Summary);
        var insightsPanel = BuildPanel(TuiScreenUtilities.GetPanelTitle(3, "Insights", navigation, 2), Pos.Right(summaryPanel), 1, Dim.Fill(), Dim.Percent(55), viewModel.NextCycle);
        var movedPanel = BuildPanel(TuiScreenUtilities.GetPanelTitle(4, "What Moved", navigation, 3), 0, Pos.Bottom(collectionsPanel), Dim.Percent(20), Dim.Fill(1), viewModel.WhatMoved);
        var blockedPanel = BuildPanel(TuiScreenUtilities.GetPanelTitle(5, "What Blocked", navigation, 4), Pos.Right(movedPanel), Pos.Bottom(summaryPanel), Dim.Percent(40), Dim.Fill(1), viewModel.WhatBlocked);
        var nextCyclePanel = BuildPanel(TuiScreenUtilities.GetPanelTitle(6, "Next Cycle", navigation, 5), Pos.Right(blockedPanel), Pos.Bottom(insightsPanel), Dim.Fill(), Dim.Fill(1), viewModel.NextCycle);
        var panels = new[] { collectionsPanel, summaryPanel, insightsPanel, movedPanel, blockedPanel, nextCyclePanel };
        var panelTitles = new[] { "Collections", "Summary", "Insights", "What Moved", "What Blocked", "Next Cycle" };
        var focusTargets = panels.Cast<View>().ToArray();
        TuiScreenUtilities.UpdatePanelTitles(panels, panelTitles, navigation);
        TuiScreenUtilities.FocusCurrentPanel(focusTargets, navigation);

        root.Add(topBar, collectionsPanel, summaryPanel, insightsPanel, movedPanel, blockedPanel, nextCyclePanel, footer);
        root.KeyPress += args =>
        {
            if (TuiScreenUtilities.IsHelpKey(args.KeyEvent))
            {
                TuiScreenUtilities.ShowContextHelp(TuiScreen.Review);
                args.Handled = true;
                return;
            }

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
                case Key x when x == (Key)'c' && onQuickCapture is not null:
                    onQuickCapture();
                    args.Handled = true;
                    break;
                case Key.q:
                    onQuit();
                    args.Handled = true;
                    break;
                case Key.Esc:
                    onBack();
                    args.Handled = true;
                    break;
            }
        };
    }

    private static FrameView BuildPanel(string title, Pos x, Pos y, Dim width, Dim height, IReadOnlyList<string> lines)
    {
        var panel = new FrameView(title) { X = x, Y = y, Width = width, Height = height };
        panel.Add(new ListView(lines.ToArray()) { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() });
        return panel;
    }
}
