using Terminal.Gui;

namespace TermBullet.Tui.Screens;

public static class ItemListScreen
{
    public static void Build(
        View root,
        string title,
        IReadOnlyList<ItemDisplayRow> rows,
        string actionsTitle,
        IReadOnlyList<string> actions,
        Action<ItemDisplayRow?> onSelectedItemChanged,
        Action<ItemDisplayRow?> onOpenDetail,
        Action<ItemDisplayRow?> onOpenMigrate,
        Action<ItemDisplayRow?> onMarkDone,
        Action<ItemDisplayRow?> onCancelItem,
        Action<ItemDisplayRow?> onDeleteItem,
        Action onBack,
        Action onQuit,
        Func<ItemDisplayRow, string>? formatRow = null,
        string footerText = " Enter open  > migrate  x done  z cancel  d delete  Tab focus  ? help  Esc back  q quit")
    {
        var selectedIndex = rows.Count > 0 ? 0 : -1;
        var selectedItem = selectedIndex >= 0 ? rows[selectedIndex] : null;
        var navigation = new Tui.Navigation.TuiNavigationState(panelCount: 3);
        onSelectedItemChanged(selectedItem);

        var topBar = new Label($" TermBullet - {title}")
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill()
        };
        var footer = new Label(footerText)
        {
            X = 0,
            Y = Pos.AnchorEnd(1),
            Width = Dim.Fill()
        };
        var itemsPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(1, title, navigation, 0))
        {
            X = 0,
            Y = 1,
            Width = Dim.Percent(52),
            Height = Dim.Fill(8)
        };
        var itemList = new ListView(TuiScreenUtilities.SanitizeListItems(BuildRows(rows, formatRow)))
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        itemsPanel.Add(itemList);

        var previewPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(2, "Preview", navigation, 1))
        {
            X = Pos.Right(itemsPanel),
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill(8)
        };
        var previewList = new ListView(TuiScreenUtilities.SanitizeListItems(BuildPreviewLines(selectedItem)))
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        previewPanel.Add(previewList);

        var actionsPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(3, actionsTitle, navigation, 2))
        {
            X = 0,
            Y = Pos.Bottom(itemsPanel),
            Width = Dim.Fill(),
            Height = 7
        };
        var actionList = new ListView(TuiScreenUtilities.SanitizeListItems(actions))
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        actionsPanel.Add(actionList);

        root.Add(topBar, itemsPanel, previewPanel, actionsPanel, footer);
        var panels = new[] { itemsPanel, previewPanel, actionsPanel };
        var panelTitles = new[] { title, "Preview", actionsTitle };
        var focusTargets = new View[] { itemList, previewList, actionList };

        itemList.SelectedItemChanged += _ =>
        {
            selectedIndex = itemList.SelectedItem;
            selectedItem = selectedIndex >= 0 && selectedIndex < rows.Count ? rows[selectedIndex] : null;
            TuiScreenUtilities.RefreshListView(previewList, BuildPreviewLines(selectedItem));
            onSelectedItemChanged(selectedItem);
        };
        itemList.KeyPress += args =>
        {
            if (TuiScreenUtilities.TryHandleEnter(args.KeyEvent.Key, () => onOpenDetail(selectedItem)))
            {
                args.Handled = true;
            }
        };

        root.KeyPress += args =>
        {
            if (TuiScreenUtilities.IsHelpKey(args.KeyEvent))
            {
                TuiScreenUtilities.ShowContextHelp(ResolveScreen(title));
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
                case Key.Enter:
                    onOpenDetail(selectedItem);
                    args.Handled = true;
                    break;
                case Key y when y == (Key)'>':
                    onOpenMigrate(selectedItem);
                    args.Handled = true;
                    break;
                case Key x when x == (Key)'x':
                    onMarkDone(selectedItem);
                    args.Handled = true;
                    break;
                case Key z when z == (Key)'z':
                    onCancelItem(selectedItem);
                    args.Handled = true;
                    break;
                case Key d when d == (Key)'d':
                    onDeleteItem(selectedItem);
                    args.Handled = true;
                    break;
                case Key.Esc:
                    onBack();
                    args.Handled = true;
                    break;
                case Key.q:
                    onQuit();
                    args.Handled = true;
                    break;
            }
        };

        itemList.SetFocus();
    }

    private static string[] BuildRows(IReadOnlyList<ItemDisplayRow> rows, Func<ItemDisplayRow, string>? formatRow)
    {
        if (rows.Count == 0)
        {
            return ["(no items)"];
        }

        return rows
            .Select(row => formatRow?.Invoke(row) ?? $"{row.Symbol} {row.PublicRef} {FormatDate(row)} {row.Content}".Trim())
            .ToArray();
    }

    internal static string[] BuildPreviewLines(ItemDisplayRow? item)
    {
        if (item is null)
        {
            return ["(nothing selected)"];
        }

        return
        [
            $"ref: {item.PublicRef}",
            $"type: {item.Type}",
            $"status: {item.Status}",
            $"collection: {item.Collection}",
            $"planned_for: {(item.PlannedFor is null ? "-" : item.PlannedFor.Value.ToString("yyyy-MM-dd"))}",
            $"scheduled_at: {(item.ScheduledAt is null ? "-" : item.ScheduledAt.Value.ToString("yyyy-MM-dd"))}",
            $"tags: {(item.Tags.Length > 0 ? string.Join(", ", item.Tags) : "(none)")}",
            " ",
            item.Content,
            "Description:",
            string.IsNullOrWhiteSpace(item.Description) ? "-" : item.Description
        ];
    }

    internal static string FormatDate(ItemDisplayRow item)
    {
        if (item.PlannedFor is not null)
        {
            return item.PlannedFor.Value.ToString("yyyy-MM-dd");
        }

        return item.ScheduledAt is null
            ? string.Empty
            : DateOnly.FromDateTime(item.ScheduledAt.Value.DateTime).ToString("yyyy-MM-dd");
    }

    private static Tui.Navigation.TuiScreen ResolveScreen(string title)
    {
        if (title.Equals("Backlog", StringComparison.OrdinalIgnoreCase))
        {
            return Tui.Navigation.TuiScreen.Backlog;
        }

        return title.Equals("Forgotten", StringComparison.OrdinalIgnoreCase)
            ? Tui.Navigation.TuiScreen.Forgotten
            : Tui.Navigation.TuiScreen.MainDashboard;
    }
}
