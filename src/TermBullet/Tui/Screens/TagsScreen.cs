using Terminal.Gui;
using TermBullet.Tui.Navigation;

namespace TermBullet.Tui.Screens;

public static class TagsScreen
{
    public static void Build(
        View root,
        TagsViewModel viewModel,
        Action onCreateTag,
        Action<string> onOpenDetail,
        Action onBack,
        Action onQuit)
    {
        var filteredTags = viewModel.Tags;
        var selectedIndex = viewModel.Tags.Count > 0 ? 0 : -1;
        var selectedTag = selectedIndex >= 0 ? viewModel.Tags[selectedIndex] : null;
        var navigation = new TuiNavigationState(panelCount: 4);

        var topBar = new Label(" TermBullet - Tags")
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill()
        };
        var footer = new Label(" Enter detail  n new  Tab/1-4 focus  Esc back  q quit")
        {
            X = 0,
            Y = Pos.AnchorEnd(1),
            Width = Dim.Fill()
        };
        var searchPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(1, "Search", navigation, 0))
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill(),
            Height = 3
        };
        var searchField = new TextField(string.Empty)
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill()
        };
        searchPanel.Add(searchField);

        var tagsPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(2, "Tags", navigation, 1))
        {
            X = 0,
            Y = Pos.Bottom(searchPanel),
            Width = Dim.Percent(52),
            Height = Dim.Fill(8)
        };
        var tagsList = new ListView(TuiScreenUtilities.SanitizeListItems(BuildRows(filteredTags)))
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        tagsPanel.Add(tagsList);

        var previewPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(3, "Preview", navigation, 2))
        {
            X = Pos.Right(tagsPanel),
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill(8)
        };
        var previewList = new ListView(TuiScreenUtilities.SanitizeListItems(BuildPreview(selectedTag)))
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        previewPanel.Add(previewList);

        var actionsPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(4, "Actions", navigation, 3))
        {
            X = 0,
            Y = Pos.Bottom(tagsPanel),
            Width = Dim.Fill(),
            Height = 7
        };
        var actionsList = new ListView(TuiScreenUtilities.SanitizeListItems(
            ["> open detail", "  create tag"]))
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        actionsPanel.Add(actionsList);

        root.Add(topBar, searchPanel, tagsPanel, previewPanel, actionsPanel, footer);

        var panels = new[] { searchPanel, tagsPanel, previewPanel, actionsPanel };
        var panelTitles = new[] { "Search", "Tags", "Preview", "Actions" };
        var focusTargets = new View[] { searchField, tagsList, previewList, actionsList };
        TuiScreenUtilities.FocusCurrentPanel(focusTargets, navigation);

        void ApplySearch()
        {
            var query = searchField.Text?.ToString() ?? string.Empty;
            filteredTags = viewModel.Filter(query);
            selectedIndex = filteredTags.Count > 0 ? 0 : -1;
            selectedTag = selectedIndex >= 0 ? filteredTags[selectedIndex] : null;
            TuiScreenUtilities.RefreshListView(tagsList, BuildRows(filteredTags));
            tagsList.SelectedItem = selectedIndex;
            TuiScreenUtilities.RefreshListView(previewList, BuildPreview(selectedTag));
        }

        searchField.TextChanged += _ => ApplySearch();

        tagsList.SelectedItemChanged += _ =>
        {
            selectedIndex = tagsList.SelectedItem;
            selectedTag = selectedIndex >= 0 && selectedIndex < filteredTags.Count
                ? filteredTags[selectedIndex]
                : null;
            TuiScreenUtilities.RefreshListView(previewList, BuildPreview(selectedTag));
        };

        bool HandleTagsShortcut(KeyEvent keyEvent)
        {
            if (TuiScreenUtilities.IsHelpKey(keyEvent))
            {
                TuiScreenUtilities.ShowContextHelp(TuiScreen.Tags);
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
                case Key c when c == (Key)'c':
                case Key n when n == (Key)'n':
                    onCreateTag();
                    return true;
                case Key.Enter when selectedTag is not null:
                    onOpenDetail(selectedTag.Name);
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
            if (HandleTagsShortcut(args.KeyEvent))
            {
                args.Handled = true;
            }
        };

        foreach (var target in focusTargets)
        {
            target.KeyPress += args =>
            {
                if (HandleTagsShortcut(args.KeyEvent))
                {
                    args.Handled = true;
                }
            };
        }
    }

    private static string[] BuildRows(IReadOnlyList<TagSummaryRow> tags) =>
        tags.Count == 0
            ? ["(no tags)"]
            : tags.Select(tag => $"# {tag.Name,-22} {tag.UsageCount} items").ToArray();

    private static string[] BuildPreview(TagSummaryRow? tag)
    {
        if (tag is null)
        {
            return ["(nothing selected)"];
        }

        return
        [
            $"name: {tag.Name}",
            $"description: {(string.IsNullOrWhiteSpace(tag.Description) ? "-" : tag.Description)}",
            $"cataloged: {(tag.IsCataloged ? "yes" : "no")}",
            $"usage: {tag.UsageCount} items",
            $"active tasks: {tag.ActiveTaskCount}",
            $"notes: {tag.NoteCount}",
            $"events: {tag.EventCount}",
            $"last used: {tag.LastUsed:yyyy-MM-dd}"
        ];
    }
}
