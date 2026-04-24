using TermBullet.Tui.Navigation;

namespace TermBullet.Tests.Tui;

public sealed class TuiScreenNavigationTests
{
    [Fact]
    public void NavigateTo_DailyFocus_changes_screen()
    {
        var state = new TuiNavigationState();

        state.NavigateTo(TuiScreen.DailyFocus, panelCount: 6);

        Assert.Equal(TuiScreen.DailyFocus, state.CurrentScreen);
    }

    [Fact]
    public void NavigateTo_BacklogTriage_changes_screen()
    {
        var state = new TuiNavigationState();

        state.NavigateTo(TuiScreen.BacklogTriage, panelCount: 6);

        Assert.Equal(TuiScreen.BacklogTriage, state.CurrentScreen);
    }

    [Fact]
    public void NavigateTo_resets_panel_focus_to_zero()
    {
        var state = new TuiNavigationState(panelCount: 3);
        state.MoveNextPanel();
        state.MoveNextPanel();

        state.NavigateTo(TuiScreen.DailyFocus, panelCount: 6);

        Assert.Equal(0, state.FocusedPanelIndex);
    }

    [Fact]
    public void CanNavigateBack_is_false_from_MainDashboard()
    {
        var state = new TuiNavigationState();

        Assert.False(state.CanNavigateBack);
    }

    [Fact]
    public void CanNavigateBack_is_true_after_navigating_away_from_MainDashboard()
    {
        var state = new TuiNavigationState();

        state.NavigateTo(TuiScreen.DailyFocus, panelCount: 6);

        Assert.True(state.CanNavigateBack);
    }

    [Fact]
    public void NavigateBack_returns_to_previous_screen()
    {
        var state = new TuiNavigationState();
        state.NavigateTo(TuiScreen.DailyFocus, panelCount: 6);

        state.NavigateBack();

        Assert.Equal(TuiScreen.MainDashboard, state.CurrentScreen);
    }

    [Fact]
    public void NavigateBack_from_MainDashboard_stays_on_MainDashboard()
    {
        var state = new TuiNavigationState();

        state.NavigateBack();

        Assert.Equal(TuiScreen.MainDashboard, state.CurrentScreen);
    }
}
