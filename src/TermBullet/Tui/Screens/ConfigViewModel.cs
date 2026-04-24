namespace TermBullet.Tui.Screens;

public sealed class ConfigViewModel
{
    private static readonly IReadOnlyList<string> AllSections = ["General", "TUI", "AI", "Calendar", "Sync"];

    private readonly IReadOnlyDictionary<string, string> _settings;
    private string _activeSection;
    private int _selectedOptionIndex;

    public ConfigViewModel(IReadOnlyDictionary<string, string> settings)
    {
        _settings = settings;
        _activeSection = "General";
        _selectedOptionIndex = OptionsForActiveSection.Count > 0 ? 0 : -1;
    }

    public IReadOnlyList<string> Sections => AllSections;

    public string ActiveSection => _activeSection;

    public int SelectedOptionIndex => _selectedOptionIndex;

    public IReadOnlyDictionary<string, string> OptionsForActiveSection =>
        _activeSection == "General"
            ? _settings
            : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public (string Key, string Value)? SelectedOption
    {
        get
        {
            if (_selectedOptionIndex < 0) return null;
            var options = OptionsForActiveSection.ToList();
            if (_selectedOptionIndex >= options.Count) return null;
            var pair = options[_selectedOptionIndex];
            return (pair.Key, pair.Value);
        }
    }

    public void ChangeSection(string section)
    {
        _activeSection = section;
        _selectedOptionIndex = OptionsForActiveSection.Count > 0 ? 0 : -1;
    }

    public void SelectNextOption()
    {
        if (_selectedOptionIndex < OptionsForActiveSection.Count - 1)
            _selectedOptionIndex++;
    }

    public void SelectPreviousOption()
    {
        if (_selectedOptionIndex > 0)
            _selectedOptionIndex--;
    }
}
