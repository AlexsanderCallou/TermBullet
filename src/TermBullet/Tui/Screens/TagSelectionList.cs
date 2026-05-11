using Terminal.Gui;

namespace TermBullet.Tui.Screens;

internal sealed class TagSelectionList
{
    private readonly List<string> _availableTags;
    private readonly HashSet<string> _selectedTags;
    private readonly ListView _listView;

    public TagSelectionList(IEnumerable<string> availableTags, IEnumerable<string>? selectedTags = null)
    {
        _availableTags =
        [
            .. availableTags
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Select(tag => tag.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(tag => tag, StringComparer.OrdinalIgnoreCase)
        ];
        _selectedTags = new HashSet<string>(
            selectedTags ?? [],
            StringComparer.OrdinalIgnoreCase);
        _listView = new ListView(TuiScreenUtilities.SanitizeListItems(BuildRows()))
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
    }

    public ListView View => _listView;

    public IReadOnlyCollection<string> SelectedTags =>
        _availableTags
            .Where(tag => _selectedTags.Contains(tag))
            .ToArray();

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
        if (!_selectedTags.Add(tag))
        {
            _selectedTags.Remove(tag);
        }

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
            .. _availableTags.Select(tag => $"{(_selectedTags.Contains(tag) ? "[x]" : "[ ]")} {tag}")
        ];
    }
}
