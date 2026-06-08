using TermBullet.Application.Items;
using TermBullet.Domain.Items;

namespace TermBullet.Tui.Screens;

public sealed class MainDashboardViewModel
{
    private int _selectedDayItemIndex;

    public MainDashboardViewModel(
        IReadOnlyCollection<ItemResult> dayItems,
        IReadOnlyCollection<ItemResult> weekItems,
        IReadOnlyCollection<ItemResult> monthItems,
        IReadOnlyCollection<ItemResult> backlogItems,
        int dailyReviewCount = 0)
    {
        DayItems = dayItems.Select(MapToRow).ToList();
        WeekItems = weekItems.Select(MapToRow).ToList();
        MonthItems = monthItems.Select(MapToRow).ToList();
        BacklogItems = backlogItems.Select(MapToRow).ToList();
        DailyReviewCount = dailyReviewCount;
        ProjectOrTagRows = dayItems.Concat(weekItems).Concat(monthItems).Concat(backlogItems)
            .Select(item => item.Tag)
            .Where(tag => !string.Equals(tag, Item.DefaultTag, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(tag => tag, StringComparer.OrdinalIgnoreCase)
            .Take(4)
            .ToArray();
        _selectedDayItemIndex = DayItems.Count > 0 ? 0 : -1;
    }

    public IReadOnlyList<ItemDisplayRow> DayItems { get; }

    public IReadOnlyList<ItemDisplayRow> WeekItems { get; }

    public IReadOnlyList<ItemDisplayRow> MonthItems { get; }

    public IReadOnlyList<ItemDisplayRow> BacklogItems { get; }

    public int DailyReviewCount { get; }

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

            var activeTag = SelectedDayItem?.Tag;
            if (string.IsNullOrWhiteSpace(activeTag) || string.Equals(activeTag, Item.DefaultTag, StringComparison.OrdinalIgnoreCase))
            {
                return BacklogItems.Take(5).ToArray();
            }

            var filtered = BacklogItems
                .Where(item => string.Equals(item.Tag, activeTag, StringComparison.OrdinalIgnoreCase))
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

    private static ItemDisplayRow MapToRow(ItemResult item) => ItemDisplayRow.From(item);
}
