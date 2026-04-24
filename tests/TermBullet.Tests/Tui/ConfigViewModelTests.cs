using TermBullet.Tui.Screens;

namespace TermBullet.Tests.Tui;

public sealed class ConfigViewModelTests
{
    [Fact]
    public void ActiveSection_starts_at_General()
    {
        var vm = new ConfigViewModel(new Dictionary<string, string>());

        Assert.Equal("General", vm.ActiveSection);
    }

    [Fact]
    public void Sections_contains_expected_entries()
    {
        var vm = new ConfigViewModel(new Dictionary<string, string>());

        Assert.Contains("General", vm.Sections);
        Assert.Contains("TUI", vm.Sections);
        Assert.Contains("AI", vm.Sections);
        Assert.Contains("Sync", vm.Sections);
    }

    [Fact]
    public void OptionsForActiveSection_returns_keys_for_General()
    {
        var settings = new Dictionary<string, string>
        {
            ["theme"] = "dark",
            ["date_format"] = "YYYY-MM-DD"
        };

        var vm = new ConfigViewModel(settings);

        Assert.Contains("theme", vm.OptionsForActiveSection.Keys);
        Assert.Contains("date_format", vm.OptionsForActiveSection.Keys);
    }

    [Fact]
    public void SelectedOptionIndex_starts_at_zero_when_options_exist()
    {
        var settings = new Dictionary<string, string> { ["theme"] = "dark" };
        var vm = new ConfigViewModel(settings);

        Assert.Equal(0, vm.SelectedOptionIndex);
    }

    [Fact]
    public void SelectedOptionIndex_is_minus_one_when_no_options()
    {
        var vm = new ConfigViewModel(new Dictionary<string, string>());

        Assert.Equal(-1, vm.SelectedOptionIndex);
    }

    [Fact]
    public void SelectNextOption_advances_selection()
    {
        var settings = new Dictionary<string, string>
        {
            ["theme"] = "dark",
            ["date_format"] = "YYYY-MM-DD"
        };
        var vm = new ConfigViewModel(settings);

        vm.SelectNextOption();

        Assert.Equal(1, vm.SelectedOptionIndex);
    }

    [Fact]
    public void SelectNextOption_does_not_advance_past_last_option()
    {
        var settings = new Dictionary<string, string> { ["theme"] = "dark" };
        var vm = new ConfigViewModel(settings);

        vm.SelectNextOption();

        Assert.Equal(0, vm.SelectedOptionIndex);
    }

    [Fact]
    public void SelectPreviousOption_moves_selection_back()
    {
        var settings = new Dictionary<string, string>
        {
            ["theme"] = "dark",
            ["date_format"] = "YYYY-MM-DD"
        };
        var vm = new ConfigViewModel(settings);
        vm.SelectNextOption();

        vm.SelectPreviousOption();

        Assert.Equal(0, vm.SelectedOptionIndex);
    }

    [Fact]
    public void ChangeSection_switches_active_section_and_resets_selection()
    {
        var settings = new Dictionary<string, string> { ["theme"] = "dark" };
        var vm = new ConfigViewModel(settings);

        vm.ChangeSection("TUI");

        Assert.Equal("TUI", vm.ActiveSection);
        Assert.Equal(-1, vm.SelectedOptionIndex);
    }

    [Fact]
    public void SelectedOption_returns_key_value_at_selected_index()
    {
        var settings = new Dictionary<string, string> { ["theme"] = "dark" };
        var vm = new ConfigViewModel(settings);

        var (key, value) = vm.SelectedOption!.Value;

        Assert.Equal("theme", key);
        Assert.Equal("dark", value);
    }

    [Fact]
    public void SelectedOption_is_null_when_no_options()
    {
        var vm = new ConfigViewModel(new Dictionary<string, string>());

        Assert.Null(vm.SelectedOption);
    }
}
