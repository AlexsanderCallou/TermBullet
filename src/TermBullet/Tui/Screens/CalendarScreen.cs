using Terminal.Gui;
using TermBullet.Tui.Navigation;

namespace TermBullet.Tui.Screens;

public static class CalendarScreen
{
    public static void Build(
        View root,
        IReadOnlyCollection<ItemDisplayRow> rows,
        Action<ItemDisplayRow?> onSelectedItemChanged,
        Action<ItemDisplayRow?> onOpenDetail,
        Action<ItemDisplayRow?> onOpenMigrate,
        Action<ItemDisplayRow?> onMarkDone,
        Action<ItemDisplayRow?> onCancelItem,
        Action<ItemDisplayRow?> onDeleteItem,
        Action onBack,
        Action onQuit)
    {
        var selectedDate = DateOnly.FromDateTime(DateTime.Today);
        var selectedItemIndex = 0;
        var navigation = new TuiNavigationState(panelCount: 4);
        var vm = CalendarViewModel.Build(rows, selectedDate);
        ItemDisplayRow? selectedItem = vm.SelectedDayItems.Count > 0 ? vm.SelectedDayItems[0] : null;
        onSelectedItemChanged(selectedItem);

        var topBar = new Label($" TermBullet - Calendar {selectedDate:MMMM yyyy}")
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill()
        };
        var footer = new Label(" Arrows day  [/] month  Enter open  > migrate  x done  z cancel  d delete  Tab/1-4 focus  Esc back  q quit")
        {
            X = 0,
            Y = Pos.AnchorEnd(1),
            Width = Dim.Fill()
        };

        var monthPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(1, "Month", navigation, 0))
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill(),
            Height = 12
        };
        var monthList = new ListView(TuiScreenUtilities.SanitizeListItems(BuildMonthLines(vm)))
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        monthPanel.Add(monthList);

        var dayItemsPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(2, "Day Items", navigation, 1))
        {
            X = 0,
            Y = Pos.Bottom(monthPanel),
            Width = Dim.Percent(52),
            Height = Dim.Fill(8)
        };
        var dayItemsList = new ListView(TuiScreenUtilities.SanitizeListItems(BuildDayItemLines(vm.SelectedDayItems)))
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        dayItemsPanel.Add(dayItemsList);

        var previewPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(3, "Preview", navigation, 2))
        {
            X = Pos.Right(dayItemsPanel),
            Y = Pos.Bottom(monthPanel),
            Width = Dim.Fill(),
            Height = Dim.Fill(8)
        };
        var previewList = new ListView(TuiScreenUtilities.SanitizeListItems(ItemListScreen.BuildPreviewLines(selectedItem)))
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
            Y = Pos.Bottom(dayItemsPanel),
            Width = Dim.Fill(),
            Height = 7
        };
        var actionsList = new ListView(TuiScreenUtilities.SanitizeListItems(
            ["> open detail", "  migrate task", "  mark done", "  cancel", "  delete"]))
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        actionsPanel.Add(actionsList);

        root.Add(topBar, monthPanel, dayItemsPanel, previewPanel, actionsPanel, footer);

        var panels = new[] { monthPanel, dayItemsPanel, previewPanel, actionsPanel };
        var panelTitles = new[] { "Month", "Day Items", "Preview", "Actions" };
        var focusTargets = new View[] { monthList, dayItemsList, previewList, actionsList };
        TuiScreenUtilities.FocusCurrentPanel(focusTargets, navigation);

        void RefreshCalendar(DateOnly date)
        {
            selectedDate = date;
            selectedItemIndex = 0;
            vm = CalendarViewModel.Build(rows, selectedDate);
            selectedItem = vm.SelectedDayItems.Count > 0 ? vm.SelectedDayItems[0] : null;
            topBar.Text = $" TermBullet - Calendar {selectedDate:MMMM yyyy}";
            TuiScreenUtilities.RefreshListView(monthList, BuildMonthLines(vm));
            TuiScreenUtilities.RefreshListView(dayItemsList, BuildDayItemLines(vm.SelectedDayItems));
            TuiScreenUtilities.RefreshListView(previewList, ItemListScreen.BuildPreviewLines(selectedItem));
            onSelectedItemChanged(selectedItem);
        }

        dayItemsList.SelectedItemChanged += _ =>
        {
            selectedItemIndex = dayItemsList.SelectedItem;
            selectedItem = selectedItemIndex >= 0 && selectedItemIndex < vm.SelectedDayItems.Count
                ? vm.SelectedDayItems[selectedItemIndex]
                : null;
            TuiScreenUtilities.RefreshListView(previewList, ItemListScreen.BuildPreviewLines(selectedItem));
            onSelectedItemChanged(selectedItem);
        };
        dayItemsList.KeyPress += args =>
        {
            if (TuiScreenUtilities.TryHandleEnter(args.KeyEvent.Key, () => onOpenDetail(selectedItem)))
            {
                args.Handled = true;
            }
        };

        bool HandleCalendarShortcut(KeyEvent keyEvent, bool includeEnter)
        {
            if (TuiScreenUtilities.IsHelpKey(keyEvent))
            {
                TuiScreenUtilities.ShowContextHelp(TuiScreen.Calendar);
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
                case Key.CursorLeft:
                    RefreshCalendar(CalendarViewModel.MoveSelectedDate(selectedDate, -1));
                    return true;
                case Key.CursorRight:
                    RefreshCalendar(CalendarViewModel.MoveSelectedDate(selectedDate, 1));
                    return true;
                case Key.CursorUp:
                    RefreshCalendar(CalendarViewModel.MoveSelectedDate(selectedDate, -7));
                    return true;
                case Key.CursorDown:
                    RefreshCalendar(CalendarViewModel.MoveSelectedDate(selectedDate, 7));
                    return true;
                case Key open when open == (Key)'[':
                    RefreshCalendar(CalendarViewModel.MoveSelectedMonth(selectedDate, -1));
                    return true;
                case Key close when close == (Key)']':
                    RefreshCalendar(CalendarViewModel.MoveSelectedMonth(selectedDate, 1));
                    return true;
                case Key.Enter when includeEnter:
                    onOpenDetail(selectedItem);
                    return true;
                case Key migrate when migrate == (Key)'>':
                    onOpenMigrate(selectedItem);
                    return true;
                case Key done when done == (Key)'x':
                    onMarkDone(selectedItem);
                    return true;
                case Key cancel when cancel == (Key)'z':
                    onCancelItem(selectedItem);
                    return true;
                case Key delete when delete == (Key)'d':
                    onDeleteItem(selectedItem);
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
            if (HandleCalendarShortcut(args.KeyEvent, includeEnter: true))
            {
                args.Handled = true;
            }
        };

        foreach (var target in focusTargets)
        {
            target.KeyPress += args =>
            {
                if (HandleCalendarShortcut(args.KeyEvent, includeEnter: false))
                {
                    args.Handled = true;
                }
            };
        }
    }

    private static string[] BuildMonthLines(CalendarViewModel vm)
    {
        var lines = new List<string> { "Mon          Tue          Wed          Thu          Fri          Sat          Sun" };
        var firstDay = new DateOnly(vm.SelectedDate.Year, vm.SelectedDate.Month, 1);
        var offset = ((int)firstDay.DayOfWeek + 6) % 7;
        var cells = Enumerable.Repeat("             ", offset)
            .Concat(vm.MonthCells.Select(FormatCell))
            .ToList();

        for (var index = 0; index < cells.Count; index += 7)
        {
            lines.Add(string.Join(" ", cells.Skip(index).Take(7)));
        }

        return [.. lines];
    }

    private static string FormatCell(CalendarDayCell cell)
    {
        var marker = cell.IsSelected ? ">" : cell.IsToday ? "*" : " ";
        var taskText = cell.TaskCount > 0 ? $"[{cell.TaskCount}]" : " - ";
        var eventText = cell.EventCount > 0 ? $"({cell.EventCount})" : string.Empty;
        return $"{marker}{cell.Date.Day:00} {taskText}{eventText}".PadRight(13);
    }

    private static string[] BuildDayItemLines(IReadOnlyList<ItemDisplayRow> rows) =>
        rows.Count == 0
            ? ["(no dated items)"]
            : rows.Select(row => $"{row.Symbol} {row.PublicRef} {row.Content}").ToArray();
}
