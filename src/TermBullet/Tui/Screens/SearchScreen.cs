using Terminal.Gui;
using TermBullet.Tui.Navigation;

namespace TermBullet.Tui.Screens;

public static class SearchScreen
{
    public static void Build(
        View root,
        SearchViewModel viewModel,
        TuiNavigationState navigation,
        Action onBack,
        Action onQuit,
        Func<string, Task> onSearch,
        Action<ItemDisplayRow?> onSelectedItemChanged,
        Action<ItemDisplayRow?> onEditSelected,
        Action<ItemDisplayRow?> onOpenSelected)
    {
        var topBar = new Label(" TermBullet - Search")
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

        var footer = new Label(" Enter search/open/action  Tab focus  ? help  Esc back")
        {
            X = 0, Y = Pos.AnchorEnd(1), Width = Dim.Fill()
        };

        var resultsPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(1, "Results", navigation, 0))
        {
            X = 0, Y = 3, Width = Dim.Percent(50), Height = Dim.Fill(5)
        };
        var resultRows = viewModel.Results.Count > 0
            ? viewModel.Results.Select(r => $"{r.Symbol} {r.PublicRef} {r.Content}").ToArray()
            : new[] { "(type a query and press Enter)" };
        var resultsList = new ListView(TuiScreenUtilities.SanitizeListItems(resultRows))
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };
        resultsPanel.Add(resultsList);

        var previewPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(2, "Preview", navigation, 1))
        {
            X = Pos.Right(resultsPanel), Y = 3, Width = Dim.Fill(), Height = Dim.Fill(5)
        };
        var selected = viewModel.SelectedResult;
        var previewLines = selected is not null
            ? new[] { $"ref: {selected.PublicRef}", $"collection: {selected.Collection}", $"priority: {selected.Priority}" }
            : new[] { "(nothing selected)" };
        var previewList = new ListView(TuiScreenUtilities.SanitizeListItems(previewLines))
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };
        previewPanel.Add(previewList);

        var actionsPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(3, "Actions", navigation, 2))
        {
            X = 0, Y = Pos.Bottom(resultsPanel), Width = Dim.Fill(), Height = 4
        };
        var actionsList = new ListView(TuiScreenUtilities.SanitizeListItems(["> open selected", "  edit selected"]))
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };
        actionsPanel.Add(actionsList);

        var panels = new[] { resultsPanel, previewPanel, actionsPanel };
        var panelTitles = new[] { "Results", "Preview", "Actions" };
        var focusTargets = new View[] { resultsList, previewList, actionsList };
        TuiScreenUtilities.UpdatePanelTitles(panels, panelTitles, navigation);

        root.Add(topBar, queryLabel, queryField, separator, resultsPanel, previewPanel, actionsPanel, footer);
        queryField.SetFocus();
        onSelectedItemChanged(viewModel.SelectedResult);

        void ActivateAction()
        {
            if (actionsList.SelectedItem == 1)
            {
                onEditSelected(viewModel.SelectedResult);
                return;
            }

            onOpenSelected(viewModel.SelectedResult);
        }

        queryField.KeyPress += args =>
        {
            if (args.KeyEvent.Key == Key.Enter)
            {
                var query = queryField.Text?.ToString() ?? string.Empty;
                viewModel.UpdateQuery(query);
                _ = Task.Run(() => onSearch(query));
                args.Handled = true;
            }
            else if (args.KeyEvent.Key == Key.Tab)
            {
                TuiScreenUtilities.UpdatePanelTitles(panels, panelTitles, navigation);
                TuiScreenUtilities.FocusCurrentPanel(focusTargets, navigation);
                args.Handled = true;
            }
            else if (args.KeyEvent.Key == Key.BackTab)
            {
                navigation.FocusPanel(3);
                TuiScreenUtilities.UpdatePanelTitles(panels, panelTitles, navigation);
                TuiScreenUtilities.FocusCurrentPanel(focusTargets, navigation);
                args.Handled = true;
            }
        };

        root.KeyPress += args =>
        {
            if (TuiScreenUtilities.ShouldLetTextInputHandle(args.KeyEvent, queryField))
            {
                return;
            }

            if (TuiScreenUtilities.IsHelpKey(args.KeyEvent))
            {
                TuiScreenUtilities.ShowContextHelp(TuiScreen.Search);
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
                case Key.Esc:
                    onBack();
                    args.Handled = true;
                    break;
                case Key.Enter:
                    if (actionsList.HasFocus)
                    {
                        ActivateAction();
                    }
                    else
                    {
                        onOpenSelected(viewModel.SelectedResult);
                    }

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
            onSelectedItemChanged(viewModel.SelectedResult);
        };
        resultsList.KeyPress += args =>
        {
            if (TuiScreenUtilities.TryHandleEnter(args.KeyEvent.Key, () => onOpenSelected(viewModel.SelectedResult)))
            {
                args.Handled = true;
            }
        };
        actionsList.KeyPress += args =>
        {
            if (TuiScreenUtilities.TryHandleEnter(args.KeyEvent.Key, ActivateAction))
            {
                args.Handled = true;
            }
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
