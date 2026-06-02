using Terminal.Gui;
using TermBullet.Domain.Items;

namespace TermBullet.Tui.Screens;

internal sealed class TagSelectionList
{
    private readonly List<string> _availableTags;
    private readonly ListView _listView;
    private string _selectedTag;

    public TagSelectionList(IEnumerable<string> availableTags, string? selectedTag = null)
    {
        _availableTags =
        [
            .. availableTags
                .Append(Item.DefaultTag)
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Select(tag => tag.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(tag => tag, StringComparer.OrdinalIgnoreCase)
        ];
        _selectedTag = string.IsNullOrWhiteSpace(selectedTag)
            ? Item.DefaultTag
            : selectedTag.Trim().ToLowerInvariant();
        _listView = new ListView(TuiScreenUtilities.SanitizeListItems(BuildRows()))
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
    }

    public ListView View => _listView;

    public string SelectedTag => _selectedTag;

    public void ToggleSelected()
    {
        if (_availableTags.Count == 0)
        {
            return;
        }

        var index = _listView.SelectedItem;
        if (index < 0 || index >= _availableTags.Count)
        {
            return;
        }

        var tag = _availableTags[index];
        _selectedTag = tag;
        Refresh();
        _listView.SelectedItem = index;
    }

    public void Refresh()
    {
        TuiScreenUtilities.RefreshListView(_listView, BuildRows());
    }

    private string[] BuildRows()
    {
        if (_availableTags.Count == 0)
        {
            return ["(no tags created)"];
        }

        return
        [
            .. _availableTags.Select(tag => $"{(string.Equals(_selectedTag, tag, StringComparison.OrdinalIgnoreCase) ? "(x)" : "( )")} {tag}")
        ];
    }
}
