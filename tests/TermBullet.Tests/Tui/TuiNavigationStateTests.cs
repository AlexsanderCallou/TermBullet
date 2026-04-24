using TermBullet.Tui.Navigation;

namespace TermBullet.Tests.Tui;

public sealed class TuiNavigationStateTests
{
    [Fact]
    public void Initial_screen_is_MainDashboard()
    {
        var state = new TuiNavigationState();

        Assert.Equal(TuiScreen.MainDashboard, state.CurrentScreen);
    }

    [Fact]
    public void Initial_focused_panel_is_first()
    {
        var state = new TuiNavigationState();

        Assert.Equal(0, state.FocusedPanelIndex);
    }

    [Fact]
    public void MoveNextPanel_advances_focus_to_next_panel()
    {
        var state = new TuiNavigationState(panelCount: 3);

        state.MoveNextPanel();

        Assert.Equal(1, state.FocusedPanelIndex);
    }

    [Fact]
    public void MoveNextPanel_wraps_focus_back_to_first_panel()
    {
        var state = new TuiNavigationState(panelCount: 3);
        state.MoveNextPanel();
        state.MoveNextPanel();
        state.MoveNextPanel();

        Assert.Equal(0, state.FocusedPanelIndex);
    }

    [Fact]
    public void MovePreviousPanel_moves_focus_back()
    {
        var state = new TuiNavigationState(panelCount: 3);
        state.MoveNextPanel();
        state.MoveNextPanel();

        state.MovePreviousPanel();

        Assert.Equal(1, state.FocusedPanelIndex);
    }

    [Fact]
    public void MovePreviousPanel_wraps_focus_to_last_panel_from_first()
    {
        var state = new TuiNavigationState(panelCount: 3);

        state.MovePreviousPanel();

        Assert.Equal(2, state.FocusedPanelIndex);
    }

    [Fact]
    public void NavigateTo_changes_current_screen_and_resets_panel_focus()
    {
        var state = new TuiNavigationState(panelCount: 3);
        state.MoveNextPanel();

        state.NavigateTo(TuiScreen.DailyFocus, panelCount: 4);

        Assert.Equal(TuiScreen.DailyFocus, state.CurrentScreen);
        Assert.Equal(0, state.FocusedPanelIndex);
    }

    [Fact]
    public void NavigateBack_restores_previous_panel_count()
    {
        var state = new TuiNavigationState(panelCount: 6);
        state.NavigateTo(TuiScreen.Search, panelCount: 2);
        state.NavigateBack();

        state.MovePreviousPanel();

        Assert.Equal(5, state.FocusedPanelIndex);
    }

    [Fact]
    public void IsPanelFocused_returns_true_for_active_panel()
    {
        var state = new TuiNavigationState(panelCount: 3);
        state.MoveNextPanel();

        Assert.True(state.IsPanelFocused(1));
        Assert.False(state.IsPanelFocused(0));
    }
}
