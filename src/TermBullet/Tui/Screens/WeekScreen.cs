using Terminal.Gui;
using TermBullet.Tui.Navigation;

namespace TermBullet.Tui.Screens;

public static class WeekScreen
{
    public static void Build(
        View root,
        IReadOnlyList<ItemDisplayRow> rows,
        Action<ItemDisplayRow?> onSelectedItemChanged,
        Action<ItemDisplayRow?> onOpenDetail,
        Action<ItemDisplayRow?> onOpenMigrate,
        Action<ItemDisplayRow?> onMarkDone,
        Action<ItemDisplayRow?> onCancelItem,
        Action<ItemDisplayRow?> onDeleteItem,
        Action onBack,
        Action onQuit)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var weekStart = GetWeekStart(today);
        var groupedRows = GroupRows(rows, weekStart);
        var selectedItem = groupedRows.SelectMany(group => group).FirstOrDefault();
        var navigation = new TuiNavigationState(panelCount: 7);
        onSelectedItemChanged(selectedItem);

        var topBar = new Label($" TermBullet - Week {weekStart:yyyy-MM-dd}..{weekStart.AddDays(6):yyyy-MM-dd}")
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill()
        };
        var footer = new Label(" Enter open  > migrate  x done  z cancel  d delete  Tab/1-7 focus  ? help  Esc back  q quit")
        {
            X = 0,
            Y = Pos.AnchorEnd(1),
            Width = Dim.Fill()
        };

        var mondayPanel = CreateDayPanel(1, DayTitle("Mon", weekStart), 0, 1, Dim.Percent(33), Dim.Percent(45), navigation, groupedRows[0], out var mondayList);
        var tuesdayPanel = CreateDayPanel(2, DayTitle("Tue", weekStart.AddDays(1)), Pos.Right(mondayPanel), 1, Dim.Percent(34), Dim.Percent(45), navigation, groupedRows[1], out var tuesdayList);
        var wednesdayPanel = CreateDayPanel(3, DayTitle("Wed", weekStart.AddDays(2)), Pos.Right(tuesdayPanel), 1, Dim.Fill(), Dim.Percent(45), navigation, groupedRows[2], out var wednesdayList);
        var thursdayPanel = CreateDayPanel(4, DayTitle("Thu", weekStart.AddDays(3)), 0, Pos.Bottom(mondayPanel), Dim.Percent(33), Dim.Fill(8), navigation, groupedRows[3], out var thursdayList);
        var fridayPanel = CreateDayPanel(5, DayTitle("Fri", weekStart.AddDays(4)), Pos.Right(thursdayPanel), Pos.Bottom(tuesdayPanel), Dim.Percent(34), Dim.Fill(8), navigation, groupedRows[4], out var fridayList);
        var weekendPanel = CreateDayPanel(6, $"{weekStart.AddDays(5):MM-dd}/{weekStart.AddDays(6):MM-dd} Weekend", Pos.Right(fridayPanel), Pos.Bottom(wednesdayPanel), Dim.Fill(), Dim.Fill(8), navigation, groupedRows[5], out var weekendList);

        var previewPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(7, "Preview", navigation, 6))
        {
            X = 0,
            Y = Pos.Bottom(thursdayPanel),
            Width = Dim.Fill(),
            Height = 7
        };
        var previewList = new ListView(TuiScreenUtilities.SanitizeListItems(ItemListScreen.BuildPreviewLines(selectedItem)))
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        previewPanel.Add(previewList);

        root.Add(topBar, mondayPanel, tuesdayPanel, wednesdayPanel, thursdayPanel, fridayPanel, weekendPanel, previewPanel, footer);

        var panels = new[] { mondayPanel, tuesdayPanel, wednesdayPanel, thursdayPanel, fridayPanel, weekendPanel, previewPanel };
        var panelTitles = new[] { DayTitle("Mon", weekStart), DayTitle("Tue", weekStart.AddDays(1)), DayTitle("Wed", weekStart.AddDays(2)), DayTitle("Thu", weekStart.AddDays(3)), DayTitle("Fri", weekStart.AddDays(4)), $"{weekStart.AddDays(5):MM-dd}/{weekStart.AddDays(6):MM-dd} Weekend", "Preview" };
        var focusTargets = new View[] { mondayList, tuesdayList, wednesdayList, thursdayList, fridayList, weekendList, previewList };
        var lists = new[] { mondayList, tuesdayList, wednesdayList, thursdayList, fridayList, weekendList };

        for (var index = 0; index < lists.Length; index++)
        {
            var dayIndex = index;
            lists[index].SelectedItemChanged += _ =>
            {
                selectedItem = ResolveSelectedItem(groupedRows[dayIndex], lists[dayIndex].SelectedItem);
                TuiScreenUtilities.RefreshListView(previewList, ItemListScreen.BuildPreviewLines(selectedItem));
                onSelectedItemChanged(selectedItem);
            };
            lists[index].KeyPress += args =>
            {
                if (TuiScreenUtilities.TryHandleEnter(args.KeyEvent.Key, () => onOpenDetail(selectedItem)))
                {
                    args.Handled = true;
                }
            };
        }

        bool HandleWeekShortcut(KeyEvent keyEvent, bool includeEnter)
        {
            if (TuiScreenUtilities.IsHelpKey(keyEvent))
            {
                TuiScreenUtilities.ShowContextHelp(TuiScreen.Week);
                return true;
            }

            if (TuiScreenUtilities.TryFocusPanelByNumber(keyEvent, navigation, panels, panelTitles, focusTargets))
            {
                selectedItem = ResolveFocusedItem(groupedRows, lists, navigation.FocusedPanelIndex) ?? selectedItem;
                TuiScreenUtilities.RefreshListView(previewList, ItemListScreen.BuildPreviewLines(selectedItem));
                onSelectedItemChanged(selectedItem);
                return true;
            }

            switch (keyEvent.Key)
            {
                case Key.Tab:
                    navigation.MoveNextPanel();
                    TuiScreenUtilities.UpdatePanelTitles(panels, panelTitles, navigation);
                    TuiScreenUtilities.FocusCurrentPanel(focusTargets, navigation);
                    selectedItem = ResolveFocusedItem(groupedRows, lists, navigation.FocusedPanelIndex) ?? selectedItem;
                    TuiScreenUtilities.RefreshListView(previewList, ItemListScreen.BuildPreviewLines(selectedItem));
                    onSelectedItemChanged(selectedItem);
                    return true;
                case Key.BackTab:
                    navigation.MovePreviousPanel();
                    TuiScreenUtilities.UpdatePanelTitles(panels, panelTitles, navigation);
                    TuiScreenUtilities.FocusCurrentPanel(focusTargets, navigation);
                    selectedItem = ResolveFocusedItem(groupedRows, lists, navigation.FocusedPanelIndex) ?? selectedItem;
                    TuiScreenUtilities.RefreshListView(previewList, ItemListScreen.BuildPreviewLines(selectedItem));
                    onSelectedItemChanged(selectedItem);
                    return true;
                case Key.Enter when includeEnter:
                    onOpenDetail(selectedItem);
                    return true;
                case Key y when y == (Key)'>':
                    onOpenMigrate(selectedItem);
                    return true;
                case Key x when x == (Key)'x':
                    onMarkDone(selectedItem);
                    return true;
                case Key z when z == (Key)'z':
                    onCancelItem(selectedItem);
                    return true;
                case Key d when d == (Key)'d':
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
            if (HandleWeekShortcut(args.KeyEvent, includeEnter: true))
            {
                args.Handled = true;
            }
        };

        foreach (var target in focusTargets)
        {
            target.KeyPress += args =>
            {
                if (HandleWeekShortcut(args.KeyEvent, includeEnter: false))
                {
                    args.Handled = true;
                }
            };
        }

        mondayList.SetFocus();
    }

    private static FrameView CreateDayPanel(
        int panelNumber,
        string title,
        Pos x,
        Pos y,
        Dim width,
        Dim height,
        TuiNavigationState navigation,
        IReadOnlyList<ItemDisplayRow> rows,
        out ListView list)
    {
        var panel = new FrameView(TuiScreenUtilities.GetPanelTitle(panelNumber, title, navigation, panelNumber - 1))
        {
            X = x,
            Y = y,
            Width = width,
            Height = height
        };
        list = new ListView(TuiScreenUtilities.SanitizeListItems(BuildDayRows(rows)))
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        panel.Add(list);
        return panel;
    }

    private static string[] BuildDayRows(IReadOnlyList<ItemDisplayRow> rows)
    {
        if (rows.Count == 0)
        {
            return ["(no items)"];
        }

        return rows
            .Select(row => $"{row.Symbol} {row.PublicRef} {FormatTime(row)} {row.Content}".Trim())
            .ToArray();
    }

    private static IReadOnlyList<ItemDisplayRow>[] GroupRows(IReadOnlyList<ItemDisplayRow> rows, DateOnly weekStart)
    {
        var groups = Enumerable.Range(0, 6).Select(_ => new List<ItemDisplayRow>()).ToArray();
        foreach (var row in rows.OrderBy(GetPlanningDate).ThenBy(row => row.ScheduledAt))
        {
            var itemDate = GetPlanningDate(row);
            if (itemDate is null || itemDate.Value < weekStart || itemDate.Value > weekStart.AddDays(6))
            {
                continue;
            }

            var offset = itemDate.Value.DayNumber - weekStart.DayNumber;
            var groupIndex = offset >= 5 ? 5 : offset;
            groups[groupIndex].Add(row);
        }

        return groups;
    }

    private static DateOnly GetWeekStart(DateOnly day)
    {
        var offsetFromMonday = ((int)day.DayOfWeek + 6) % 7;
        return day.AddDays(-offsetFromMonday);
    }

    private static DateOnly? GetPlanningDate(ItemDisplayRow row)
    {
        return row.ScheduledAt is null
            ? null
            : DateOnly.FromDateTime(row.ScheduledAt.Value.DateTime);
    }

    private static string FormatTime(ItemDisplayRow row)
    {
        if (row.ScheduledAt is null)
        {
            return string.Empty;
        }

        return row.ScheduledAt.Value.ToString("HH:mm");
    }

    private static string DayTitle(string label, DateOnly date) => $"{date:MM-dd} {label}";

    private static ItemDisplayRow? ResolveFocusedItem(
        IReadOnlyList<ItemDisplayRow>[] groupedRows,
        IReadOnlyList<ListView> lists,
        int focusedPanelIndex)
    {
        if (focusedPanelIndex < 0 || focusedPanelIndex >= groupedRows.Length)
        {
            return null;
        }

        return ResolveSelectedItem(groupedRows[focusedPanelIndex], lists[focusedPanelIndex].SelectedItem);
    }

    private static ItemDisplayRow? ResolveSelectedItem(IReadOnlyList<ItemDisplayRow> rows, int selectedIndex) =>
        selectedIndex >= 0 && selectedIndex < rows.Count
            ? rows[selectedIndex]
            : null;
}
