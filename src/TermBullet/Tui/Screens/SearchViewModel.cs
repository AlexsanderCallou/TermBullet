using TermBullet.Application.Items;
using TermBullet.Core.Items;

namespace TermBullet.Tui.Screens;

public sealed class SearchViewModel
{
    private List<ItemDisplayRow> _results = [];
    private int _selectedItemIndex = -1;

    public string Query { get; private set; } = string.Empty;

    public IReadOnlyList<ItemDisplayRow> Results => _results;

    public int SelectedItemIndex => _selectedItemIndex;

    public ItemDisplayRow? SelectedResult =>
        _selectedItemIndex >= 0 ? _results[_selectedItemIndex] : null;

    public void UpdateQuery(string query)
    {
        Query = query;
        _results = [];
        _selectedItemIndex = -1;
    }

    public void SetResults(IReadOnlyCollection<ItemResult> items)
    {
        _results = items.Select(MapToRow).ToList();
        _selectedItemIndex = _results.Count > 0 ? 0 : -1;
    }

    public void SelectNextResult()
    {
        if (_selectedItemIndex < _results.Count - 1)
            _selectedItemIndex++;
    }

    public void SelectPreviousResult()
    {
        if (_selectedItemIndex > 0)
            _selectedItemIndex--;
    }

    private static ItemDisplayRow MapToRow(ItemResult item) =>
        new()
        {
            PublicRef = item.PublicRef,
            Symbol = item.Type switch
            {
                ItemType.Note => "(.)",
                ItemType.Event => "(o)",
                _ => item.Status switch
                {
                    ItemStatus.Open => "[ ]",
                    ItemStatus.InProgress => "[~]",
                    ItemStatus.Done => "[x]",
                    ItemStatus.Cancelled => "[-]",
                    ItemStatus.Migrated => "[>]",
                    _ => "[ ]"
                }
            },
            Type = item.Type.ToString().ToLowerInvariant(),
            Status = item.Status.ToString().ToLowerInvariant(),
            Content = item.Content,
            Priority = item.Priority.ToString().ToLowerInvariant(),
            Collection = item.Collection.ToString().ToLowerInvariant(),
            Tags = [.. item.Tags]
        };
}
