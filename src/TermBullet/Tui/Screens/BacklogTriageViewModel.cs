using TermBullet.Application.Items;
using TermBullet.Core.Items;

namespace TermBullet.Tui.Screens;

public sealed class BacklogTriageViewModel
{
    private readonly IReadOnlyList<BacklogTriageItemRow> _allItems;
    private List<BacklogTriageItemRow> _filteredItems;
    private int _selectedItemIndex;

    public BacklogTriageViewModel(IReadOnlyCollection<ItemResult> items)
    {
        _allItems = items.Select(MapToRow).ToList();
        AvailableTags = _allItems
            .SelectMany(r => r.Tags)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t, StringComparer.OrdinalIgnoreCase)
            .ToList();

        _filteredItems = [.. _allItems];
        _selectedItemIndex = _filteredItems.Count > 0 ? 0 : -1;
    }

public IReadOnlyList<BacklogTriageItemRow> FilteredItems => _filteredItems;

    public IReadOnlyList<string> AvailableTags { get; }

    public string? ActiveTagFilter { get; private set; }

    public Priority? ActivePriorityFilter { get; private set; }

    public int SelectedItemIndex => _selectedItemIndex;

    public BacklogTriageItemRow? SelectedItem =>
        _selectedItemIndex >= 0 ? _filteredItems[_selectedItemIndex] : null;

    public void SetTagFilter(string tag)
    {
        ActiveTagFilter = tag;
        ApplyFilters();
    }

    public void SetPriorityFilter(Priority priority)
    {
        ActivePriorityFilter = priority;
        ApplyFilters();
    }

    public void ClearFilters()
    {
        ActiveTagFilter = null;
        ActivePriorityFilter = null;
        ApplyFilters();
    }

    public void SelectNextItem()
    {
        if (_selectedItemIndex < _filteredItems.Count - 1)
        {
            _selectedItemIndex++;
        }
    }

    public void SelectPreviousItem()
    {
        if (_selectedItemIndex > 0)
        {
            _selectedItemIndex--;
        }
    }

    private void ApplyFilters()
    {
        _filteredItems = _allItems
            .Where(r => ActiveTagFilter is null ||
                r.Tags.Any(t => string.Equals(t, ActiveTagFilter, StringComparison.OrdinalIgnoreCase)))
            .Where(r => ActivePriorityFilter is null ||
                string.Equals(r.Priority, ActivePriorityFilter.Value.ToString().ToLowerInvariant(),
                    StringComparison.OrdinalIgnoreCase))
            .ToList();

        _selectedItemIndex = _filteredItems.Count > 0 ? 0 : -1;
    }

    private static BacklogTriageItemRow MapToRow(ItemResult item) =>
        new()
        {
            PublicRef = item.PublicRef,
            Symbol = item.Status switch
            {
                ItemStatus.Open => "[ ]",
                ItemStatus.InProgress => "[~]",
                ItemStatus.Done => "[x]",
                ItemStatus.Cancelled => "[-]",
                ItemStatus.Migrated => "[>]",
                _ => "[ ]"
            },
            Content = item.Content,
            Priority = item.Priority.ToString().ToLowerInvariant(),
            Collection = item.Collection.ToString().ToLowerInvariant(),
            Tags = [.. item.Tags]
        };
}
