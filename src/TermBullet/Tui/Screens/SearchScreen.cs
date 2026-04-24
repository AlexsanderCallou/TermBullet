using Terminal.Gui;
using TermBullet.Tui.Navigation;
using TGui = Terminal.Gui.Application;

namespace TermBullet.Tui.Screens;

public static class SearchScreen
{
    public static void Build(
        Toplevel top,
        SearchViewModel viewModel,
        TuiNavigationState navigation,
        Action onBack,
        Func<string, Task> onSearch)
    {
        var topBar = new Label(" TermBullet \u2500 Search \u2500 data:local \u2500 ai:off \u2500 sync:idle \u2500 mode:search")
        {
            X = 0, Y = 0, Width = Dim.Fill()
        };

        var queryLabel = new Label(" query: ")
        {
            X = 0, Y = 1
        };
        var queryField = new TextField(viewModel.Query)
        {
            X = Pos.Right(queryLabel), Y = 1,
            Width = Dim.Fill()
        };

        var separator = new Label(new string('\u2500', 80))
        {
            X = 0, Y = 2, Width = Dim.Fill()
        };

        var footer = new Label(" / search  Enter open  Ctrl+e edit  Ctrl+x done  Tab focus  ? help  Esc back")
        {
            X = 0, Y = Pos.AnchorEnd(1), Width = Dim.Fill()
        };

        // Panel 1: Results
        var resultsPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(1, "Results", navigation, 0))
        {
            X = 0, Y = 3, Width = Dim.Percent(55), Height = Dim.Fill(1)
        };
        var resultRows = viewModel.Results.Count > 0
            ? viewModel.Results.Select(r => $"{r.Symbol} {r.PublicRef} {r.Content}").ToArray()
            : new[] { "(type a query and press Enter)" };
        var resultsList = new ListView(resultRows)
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };
        resultsPanel.Add(resultsList);

        // Panel 2: Preview
        var previewPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(2, "Preview", navigation, 1))
        {
            X = Pos.Right(resultsPanel), Y = 3, Width = Dim.Fill(), Height = Dim.Fill(1)
        };
        var selected = viewModel.SelectedResult;
        var previewLines = selected is not null
            ? new[] { $"ref: {selected.PublicRef}", $"collection: {selected.Collection}", $"priority: {selected.Priority}" }
            : new[] { "(nothing selected)" };
        var previewList = new ListView(previewLines)
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };
        previewPanel.Add(previewList);

        var panels = new[] { resultsPanel, previewPanel };
        var panelTitles = new[] { "Results", "Preview" };
        var focusTargets = new View[] { queryField, previewList };
        TuiScreenUtilities.UpdatePanelTitles(panels, panelTitles, navigation);

        top.Add(topBar, queryLabel, queryField, separator, resultsPanel, previewPanel, footer);
        TuiScreenUtilities.FocusCurrentPanel(focusTargets, navigation);

        queryField.KeyPress += args =>
        {
            if (args.KeyEvent.Key == Key.Enter)
            {
                var query = queryField.Text?.ToString() ?? string.Empty;
                viewModel.UpdateQuery(query);
                _ = Task.Run(async () =>
                {
                    await onSearch(query);
                    TGui.MainLoop?.Invoke(() => TGui.RequestStop());
                });
                args.Handled = true;
            }
        };

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
                    TuiScreenUtilities.ShowContextHelp(TuiScreen.Search);
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

        resultsList.SelectedItemChanged += _ =>
        {
            var diff = resultsList.SelectedItem - viewModel.SelectedItemIndex;
            if (diff > 0)
                for (var i = 0; i < diff; i++) viewModel.SelectNextResult();
            else if (diff < 0)
                for (var i = 0; i < -diff; i++) viewModel.SelectPreviousResult();

            TuiScreenUtilities.RefreshListView(previewList, BuildPreviewLines(viewModel.SelectedResult));
        };
    }

    private static string[] BuildPreviewLines(ItemDisplayRow? selected) =>
        selected is not null
            ?
            [
                $"ref: {selected.PublicRef}",
                $"collection: {selected.Collection}",
                $"priority: {selected.Priority}",
                $"status: {selected.Status}"
            ]
            : ["(nothing selected)"];
}
