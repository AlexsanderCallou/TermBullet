using Terminal.Gui;
using TermBullet.Tui.Navigation;

namespace TermBullet.Tui.Screens;

public static class TagDetailScreen
{
    public static void Build(
        View root,
        TagDetailViewModel viewModel,
        TuiNavigationState navigation,
        Action<ItemDisplayRow?> onOpenDetail,
        Action<ItemDisplayRow?> onEditItem,
        Action<string> onCreateItem,
        Action<string> onQuickTask,
        Action onBack,
        Action onQuit)
    {
        var topBar = new Label($" TermBullet - Tag #{viewModel.Tag}") { X = 0, Y = 0, Width = Dim.Fill() };
        var footer = new Label(" Enter detail  c create  n quick task  e edit item  Tab/1-5 focus  Esc back  q quit")
        {
            X = 0,
            Y = Pos.AnchorEnd(1),
            Width = Dim.Fill()
        };

        var topHeight = Dim.Percent(30);
        var summaryPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(1, "Summary", navigation, 0))
        {
            X = 0, Y = 1, Width = Dim.Percent(50), Height = topHeight
        };
        var timelinePanel = new FrameView(TuiScreenUtilities.GetPanelTitle(2, "Timeline", navigation, 1))
        {
            X = Pos.Right(summaryPanel), Y = 1, Width = Dim.Fill(), Height = topHeight
        };
        var tasksPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(3, "Tasks", navigation, 2))
        {
            X = 0, Y = Pos.Bottom(summaryPanel), Width = Dim.Fill(), Height = Dim.Percent(42)
        };
        var notesPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(4, "Notes", navigation, 3))
        {
            X = 0, Y = Pos.Bottom(tasksPanel), Width = Dim.Percent(50), Height = Dim.Fill(1)
        };
        var eventsPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(5, "Events", navigation, 4))
        {
            X = Pos.Right(notesPanel), Y = Pos.Bottom(tasksPanel), Width = Dim.Fill(), Height = Dim.Fill(1)
        };

        var summaryLines = viewModel.SummaryLines.ToArray();
        var timelineLines = viewModel.TimelineLines.ToArray();
        var taskLines = viewModel.TaskLines.ToArray();
        var noteLines = viewModel.NoteLines.ToArray();
        var eventLines = viewModel.EventLines.ToArray();

        var summaryList = AddList(summaryPanel, summaryLines);
        var timelineList = AddList(timelinePanel, timelineLines);
        var tasksList = AddList(tasksPanel, taskLines);
        var notesList = AddList(notesPanel, noteLines);
        var eventsList = AddList(eventsPanel, eventLines);

        var panels = new[] { summaryPanel, timelinePanel, tasksPanel, notesPanel, eventsPanel };
        var panelTitles = new[] { "Summary", "Timeline", "Tasks", "Notes", "Events" };
        var focusTargets = new View[] { summaryList, timelineList, tasksList, notesList, eventsList };
        TuiScreenUtilities.UpdatePanelTitles(panels, panelTitles, navigation);
        root.Add(topBar, summaryPanel, timelinePanel, tasksPanel, notesPanel, eventsPanel, footer);
        TuiScreenUtilities.FocusCurrentPanel(focusTargets, navigation);

        ItemDisplayRow? SelectedItem() =>
            navigation.FocusedPanelIndex switch
            {
                2 => ResolveSelectedItem(taskLines, tasksList.SelectedItem, viewModel.SelectableItems),
                3 => ResolveSelectedItem(noteLines, notesList.SelectedItem, viewModel.SelectableItems),
                4 => ResolveSelectedItem(eventLines, eventsList.SelectedItem, viewModel.SelectableItems),
                _ => null
            };

        bool HandleShortcut(KeyEvent keyEvent)
        {
            if (TuiScreenUtilities.IsHelpKey(keyEvent))
            {
                TuiScreenUtilities.ShowContextHelp(TuiScreen.TagDetail);
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
                case Key.Enter:
                    onOpenDetail(SelectedItem());
                    return true;
                case Key c when c == (Key)'c':
                    onCreateItem(viewModel.Tag);
                    return true;
                case Key n when n == (Key)'n':
                    onQuickTask(viewModel.Tag);
                    return true;
                case Key e when e == (Key)'e':
                    onEditItem(SelectedItem());
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
            if (HandleShortcut(args.KeyEvent))
            {
                args.Handled = true;
            }
        };

        foreach (var target in focusTargets)
        {
            target.KeyPress += args =>
            {
                if (HandleShortcut(args.KeyEvent))
                {
                    args.Handled = true;
                }
            };
        }
    }

    private static ListView AddList(FrameView parent, IReadOnlyList<string> lines)
    {
        var list = new ListView(TuiScreenUtilities.SanitizeListItems(lines))
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        parent.Add(list);
        return list;
    }

    private static ItemDisplayRow? ResolveSelectedItem(
        IReadOnlyList<string> lines,
        int selectedIndex,
        IReadOnlyList<ItemDisplayRow> items)
    {
        if (selectedIndex < 0 || selectedIndex >= lines.Count)
        {
            return null;
        }

        var selectedLine = lines[selectedIndex];
        return items.FirstOrDefault(item => selectedLine.Contains(item.PublicRef, StringComparison.Ordinal));
    }
}
