using Terminal.Gui;
using TermBullet.Tui.Navigation;
using TGui = Terminal.Gui.Application;

namespace TermBullet.Tui.Screens;

public static class BacklogTriageScreen
{
    public static void Build(
        Toplevel top,
        BacklogTriageViewModel viewModel,
        TuiNavigationState navigation,
        Action onBack,
        Action? onQuickCapture = null)
    {
        var topBar = new Label(" TermBullet \u2500 Backlog \u2500 data:local \u2500 ai:off \u2500 sync:idle \u2500 mode:filter")
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill()
        };

        var footer = new Label(" / filter  c capture  > migrate  Enter zoom  Tab focus  ? help  q quit")
        {
            X = 0,
            Y = Pos.AnchorEnd(1),
            Width = Dim.Fill()
        };

        // Panel 1: Filters
        var filtersPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(1, "Filters", navigation, 0))
        {
            X = 0,
            Y = 1,
            Width = Dim.Percent(22),
            Height = Dim.Fill(1)
        };

        var filterEntries = new List<string> { "  status: open", "  priority: all" };
        filterEntries.AddRange(viewModel.AvailableTags.Select(t => $"  tag: {t}"));
        var filtersList = new ListView(filterEntries.ToArray())
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };
        filtersPanel.Add(filtersList);

        // Panel 2: Backlog Items
        var itemsPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(2, "Backlog Items", navigation, 1))
        {
            X = Pos.Right(filtersPanel),
            Y = 1,
            Width = Dim.Percent(50),
            Height = Dim.Fill(1)
        };
        var itemRows = viewModel.FilteredItems.Count > 0
            ? viewModel.FilteredItems.Select(r => $"{r.Symbol} {r.PublicRef} {r.Content}").ToArray()
            : new[] { "(no items)" };
        var itemsList = new ListView(itemRows)
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };
        itemsPanel.Add(itemsList);

        // Panel 3: Preview
        var previewPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(3, "Preview", navigation, 2))
        {
            X = Pos.Right(itemsPanel),
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill(1)
        };
        var selected = viewModel.SelectedItem;
        var previewLines = selected is not null
            ? new[]
            {
                selected.PublicRef,
                $"priority: {selected.Priority}",
                $"collection: {selected.Collection}",
                $"tags: {string.Join(", ", selected.Tags)}"
            }
            : new[] { "(nothing selected)" };
        var previewList = new ListView(previewLines)
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };
        previewPanel.Add(previewList);

        var panels = new[] { filtersPanel, itemsPanel, previewPanel };
        var panelTitles = new[] { "Filters", "Backlog Items", "Preview" };
        var focusTargets = new View[] { filtersList, itemsList, previewList };
        TuiScreenUtilities.UpdatePanelTitles(panels, panelTitles, navigation);
        TuiScreenUtilities.FocusCurrentPanel(focusTargets, navigation);

        top.Add(topBar, filtersPanel, itemsPanel, previewPanel, footer);

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
                    TuiScreenUtilities.ShowContextHelp(TuiScreen.BacklogTriage);
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

        itemsList.SelectedItemChanged += _ =>
        {
            var diff = itemsList.SelectedItem - viewModel.SelectedItemIndex;
            if (diff > 0)
                for (var i = 0; i < diff; i++) viewModel.SelectNextItem();
            else if (diff < 0)
                for (var i = 0; i < -diff; i++) viewModel.SelectPreviousItem();

            TuiScreenUtilities.RefreshListView(previewList, BuildPreviewLines(viewModel.SelectedItem));
        };
    }

    private static string[] BuildPreviewLines(BacklogTriageItemRow? selected) =>
        selected is not null
            ?
            [
                selected.PublicRef,
                $"priority: {selected.Priority}",
                $"collection: {selected.Collection}",
                $"tags: {(selected.Tags.Length > 0 ? string.Join(", ", selected.Tags) : "(none)")}"
            ]
            : ["(nothing selected)"];
}
