using TermBullet.Application.Items;
using TermBullet.Core.Items;

namespace TermBullet.Tui.Screens;

public sealed class MainDashboardViewModel
{
    private int _selectedDayItemIndex;

    public MainDashboardViewModel(
        IReadOnlyCollection<ItemResult> dayItems,
        IReadOnlyCollection<ItemResult> backlogItems)
    {
        DayItems = dayItems.Select(MapToRow).ToList();
        BacklogItems = backlogItems.Select(MapToRow).ToList();
        ProjectOrTagRows = dayItems.Concat(backlogItems)
            .SelectMany(item => item.Tags)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(tag => tag, StringComparer.OrdinalIgnoreCase)
            .Take(6)
            .ToArray();
        _selectedDayItemIndex = DayItems.Count > 0 ? 0 : -1;
    }

    public IReadOnlyList<ItemDisplayRow> DayItems { get; }

    public IReadOnlyList<ItemDisplayRow> BacklogItems { get; }

    public IReadOnlyList<string> ProjectOrTagRows { get; }

    public int SelectedDayItemIndex => _selectedDayItemIndex;

    public ItemDisplayRow? SelectedDayItem =>
        _selectedDayItemIndex >= 0 ? DayItems[_selectedDayItemIndex] : null;

    public IReadOnlyList<ItemDisplayRow> FilteredBacklogItems
    {
        get
        {
            if (BacklogItems.Count == 0)
            {
                return [];
            }

            var activeTags = SelectedDayItem?.Tags ?? [];
            if (activeTags.Length == 0)
            {
                return BacklogItems.Take(5).ToArray();
            }

            var filtered = BacklogItems
                .Where(item => item.Tags.Any(tag => activeTags.Contains(tag, StringComparer.OrdinalIgnoreCase)))
                .Take(5)
                .ToArray();

            return filtered.Length > 0 ? filtered : BacklogItems.Take(5).ToArray();
        }
    }

    public IReadOnlyList<string> SuggestedPlanLines
    {
        get
        {
            if (SelectedDayItem is null)
            {
                return
                [
                    "focus: capture first actionable item",
                    "next: review backlog context",
                    "avoid: broad refactor before triage"
                ];
            }

            return
            [
                $"focus: {SelectedDayItem.Content}",
                $"next: resolve {SelectedDayItem.PublicRef}",
                SelectedDayItem.Priority == "high"
                    ? "avoid: opening parallel work before closing this item"
                    : "avoid: losing context while switching collections"
            ];
        }
    }

    public void SelectNextDayItem()
    {
        if (_selectedDayItemIndex < DayItems.Count - 1)
        {
            _selectedDayItemIndex++;
        }
    }

    public void SelectPreviousDayItem()
    {
        if (_selectedDayItemIndex > 0)
        {
            _selectedDayItemIndex--;
        }
    }

    private static ItemDisplayRow MapToRow(ItemResult item) =>
        new()
        {
            PublicRef = item.PublicRef,
            Symbol = ResolveSymbol(item.Type, item.Status),
            Type = item.Type.ToString().ToLowerInvariant(),
            Status = item.Status.ToString().ToLowerInvariant(),
            Content = item.Content,
            Priority = item.Priority.ToString().ToLowerInvariant(),
            Collection = item.Collection.ToString().ToLowerInvariant(),
            Tags = [.. item.Tags]
        };

    private static string ResolveSymbol(ItemType type, ItemStatus status) =>
        type switch
        {
            ItemType.Note => "(.)",
            ItemType.Event => "(o)",
            _ => status switch
            {
                ItemStatus.Open => "[ ]",
                ItemStatus.InProgress => "[~]",
                ItemStatus.Done => "[x]",
                ItemStatus.Cancelled => "[-]",
                ItemStatus.Migrated => "[>]",
                _ => "[ ]"
            }
        };
}
