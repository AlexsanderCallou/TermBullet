using TermBullet.Application.Items;
using TermBullet.Core.Items;

namespace TermBullet.Tui.Screens;

public sealed class DailyFocusViewModel
{
    private DailyFocusSection _activeSection;
    private int _selectedItemIndex;

    public DailyFocusViewModel(IReadOnlyCollection<ItemResult> items)
    {
        OpenItems = items.Where(i => i.Status == ItemStatus.Open)
            .Select(MapToRow).ToList();
        InProgressItems = items.Where(i => i.Status == ItemStatus.InProgress)
            .Select(MapToRow).ToList();
        DoneItems = items.Where(i => i.Status == ItemStatus.Done)
            .Select(MapToRow).ToList();
        CancelledItems = items.Where(i => i.Status == ItemStatus.Cancelled)
            .Select(MapToRow).ToList();
        MigratedItems = items.Where(i => i.Status == ItemStatus.Migrated)
            .Select(MapToRow).ToList();

        _activeSection = DailyFocusSection.Open;
        _selectedItemIndex = ActiveSectionItems.Count > 0 ? 0 : -1;
    }

    public IReadOnlyList<ItemDisplayRow> OpenItems { get; }

    public IReadOnlyList<ItemDisplayRow> InProgressItems { get; }

    public IReadOnlyList<ItemDisplayRow> DoneItems { get; }

    public IReadOnlyList<ItemDisplayRow> CancelledItems { get; }

    public IReadOnlyList<ItemDisplayRow> MigratedItems { get; }

    public DailyFocusSection ActiveSection => _activeSection;

    public int SelectedItemIndex => _selectedItemIndex;

    public IReadOnlyList<ItemDisplayRow> ActiveSectionItems => _activeSection switch
    {
        DailyFocusSection.Open => OpenItems,
        DailyFocusSection.InProgress => InProgressItems,
        DailyFocusSection.Done => DoneItems,
        DailyFocusSection.Cancelled => CancelledItems,
        DailyFocusSection.Migrated => MigratedItems,
        _ => OpenItems
    };

    public IReadOnlyList<string> QuickCaptureExamples =>
    [
        "- review swagger",
        ". empty audience bug",
        "o sync 16:00",
        "[Enter to add]"
    ];

    public IReadOnlyList<string> ShortHistoryLines =>
    [
        "latest changes appear here after persistence",
        "history is available in monthly files",
        "use CLI for full history clear"
    ];

    public IReadOnlyList<string> ActionHints =>
    [
        "x done",
        "> migrate",
        "e edit",
        "d delete"
    ];

    public ItemDisplayRow? SelectedItem =>
        _selectedItemIndex >= 0 ? ActiveSectionItems[_selectedItemIndex] : null;

    public void SelectNextItem()
    {
        if (_selectedItemIndex < ActiveSectionItems.Count - 1)
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

    public void ChangeSection(DailyFocusSection section)
    {
        _activeSection = section;
        _selectedItemIndex = ActiveSectionItems.Count > 0 ? 0 : -1;
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
