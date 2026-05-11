using Terminal.Gui;
using TermBullet.Tui.Navigation;

namespace TermBullet.Tui.Screens;

public static class TagsScreen
{
    public static void Build(
        View root,
        TagsViewModel viewModel,
        Action onCreateTag,
        Action onBack,
        Action onQuit)
    {
        var selectedIndex = viewModel.Tags.Count > 0 ? 0 : -1;
        var selectedTag = selectedIndex >= 0 ? viewModel.Tags[selectedIndex] : null;
        var navigation = new TuiNavigationState(panelCount: 3);

        var topBar = new Label(" TermBullet - Tags")
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill()
        };
        var footer = new Label(" c create  Enter preview  Tab/1-3 focus  ? help  Esc back  q quit")
        {
            X = 0,
            Y = Pos.AnchorEnd(1),
            Width = Dim.Fill()
        };
        var tagsPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(1, "Tags", navigation, 0))
        {
            X = 0,
            Y = 1,
            Width = Dim.Percent(52),
            Height = Dim.Fill(8)
        };
        var tagsList = new ListView(TuiScreenUtilities.SanitizeListItems(BuildRows(viewModel.Tags)))
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        tagsPanel.Add(tagsList);

        var previewPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(2, "Preview", navigation, 1))
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

        var actionsPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(3, "Actions", navigation, 2))
        {
            X = 0,
            Y = Pos.Bottom(tagsPanel),
            Width = Dim.Fill(),
            Height = 7
        };
        var actionsList = new ListView(TuiScreenUtilities.SanitizeListItems(
            ["> create tag", "  preview selected", "  remove selected from all items: future rule"]))
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        actionsPanel.Add(actionsList);

        root.Add(topBar, tagsPanel, previewPanel, actionsPanel, footer);

        var panels = new[] { tagsPanel, previewPanel, actionsPanel };
        var panelTitles = new[] { "Tags", "Preview", "Actions" };
        var focusTargets = new View[] { tagsList, previewList, actionsList };
        TuiScreenUtilities.FocusCurrentPanel(focusTargets, navigation);

        tagsList.SelectedItemChanged += _ =>
        {
            selectedIndex = tagsList.SelectedItem;
            selectedTag = selectedIndex >= 0 && selectedIndex < viewModel.Tags.Count
                ? viewModel.Tags[selectedIndex]
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
                    onCreateTag();
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
            : tags.Select(tag => $"{tag.Name,-24} {tag.UsageCount} items").ToArray();

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
