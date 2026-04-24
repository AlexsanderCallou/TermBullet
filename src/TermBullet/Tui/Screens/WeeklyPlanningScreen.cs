using Terminal.Gui;
using TermBullet.Tui.Navigation;
using TGui = Terminal.Gui.Application;

namespace TermBullet.Tui.Screens;

public static class WeeklyPlanningScreen
{
    public static void Build(
        Toplevel top,
        WeeklyPlanningViewModel viewModel,
        TuiNavigationState navigation,
        Action onBack,
        Action? onQuickCapture = null)
    {
        var weekLabel = $"{DateTime.Today:yyyy}-W{System.Globalization.ISOWeek.GetWeekOfYear(DateTime.Today):00}";
        var topBar = new Label($" TermBullet \u2500 Weekly {weekLabel} \u2500 data:local \u2500 ai:off \u2500 sync:idle \u2500 mode:normal")
        {
            X = 0, Y = 0, Width = Dim.Fill()
        };

        var footer = new Label(" / filter  c capture  p plan  > migrate  Enter zoom  Tab focus  ? help  q quit")
        {
            X = 0, Y = Pos.AnchorEnd(1), Width = Dim.Fill()
        };

        var bucketsPanel = BuildPanel(TuiScreenUtilities.GetPanelTitle(1, "Buckets", navigation, 0), 0, 1, Dim.Percent(20), Dim.Percent(55), viewModel.Buckets);
        var weekPanel = BuildPanel(TuiScreenUtilities.GetPanelTitle(2, "Week", navigation, 1), Pos.Right(bucketsPanel), 1, Dim.Percent(50), Dim.Percent(55),
            viewModel.WeekItems.Count > 0
                ? viewModel.WeekItems.Select(item => $"{item.Symbol} {item.PublicRef} {item.Content}").ToArray()
                : ["(no week items)"]);
        var contextPanel = BuildPanel(TuiScreenUtilities.GetPanelTitle(3, "Context / AI", navigation, 2), Pos.Right(weekPanel), 1, Dim.Fill(), Dim.Percent(55), viewModel.Notes);
        var metricsPanel = BuildPanel(TuiScreenUtilities.GetPanelTitle(4, "Metrics", navigation, 3), 0, Pos.Bottom(bucketsPanel), Dim.Percent(20), Dim.Fill(1), viewModel.Metrics);
        var backlogPanel = BuildPanel(TuiScreenUtilities.GetPanelTitle(5, "Week Backlog", navigation, 4), Pos.Right(metricsPanel), Pos.Bottom(weekPanel), Dim.Percent(50), Dim.Fill(1),
            viewModel.BacklogItems.Count > 0
                ? viewModel.BacklogItems.Select(item => $"{item.PublicRef} {item.Content}").ToArray()
                : ["(no backlog context)"]);
        var notesPanel = BuildPanel(TuiScreenUtilities.GetPanelTitle(6, "Notes", navigation, 5), Pos.Right(backlogPanel), Pos.Bottom(contextPanel), Dim.Fill(), Dim.Fill(1), viewModel.Notes);
        var panels = new[] { bucketsPanel, weekPanel, contextPanel, metricsPanel, backlogPanel, notesPanel };
        var panelTitles = new[] { "Buckets", "Week", "Context / AI", "Metrics", "Week Backlog", "Notes" };
        var focusTargets = panels.Cast<View>().ToArray();
        TuiScreenUtilities.UpdatePanelTitles(panels, panelTitles, navigation);
        TuiScreenUtilities.FocusCurrentPanel(focusTargets, navigation);

        top.Add(topBar, bucketsPanel, weekPanel, contextPanel, metricsPanel, backlogPanel, notesPanel, footer);
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
                    TuiScreenUtilities.ShowContextHelp(TuiScreen.WeeklyPlanning);
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
    }

    private static FrameView BuildPanel(string title, Pos x, Pos y, Dim width, Dim height, IReadOnlyList<string> lines)
    {
        var panel = new FrameView(title) { X = x, Y = y, Width = width, Height = height };
        panel.Add(new ListView(lines.ToArray()) { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() });
        return panel;
    }
}
