namespace TermBullet.Tui.Navigation;

public sealed class TuiNavigationState
{
    private int _panelCount;
    private readonly Stack<(TuiScreen Screen, int PanelCount)> _history = new();

    public TuiNavigationState(int panelCount = 6)
    {
        _panelCount = panelCount;
        CurrentScreen = TuiScreen.MainDashboard;
        FocusedPanelIndex = 0;
    }

    public TuiScreen CurrentScreen { get; private set; }

    public int FocusedPanelIndex { get; private set; }

    public bool CanNavigateBack => _history.Count > 0;

    public void MoveNextPanel()
    {
        FocusedPanelIndex = (FocusedPanelIndex + 1) % _panelCount;
    }

    public void MovePreviousPanel()
    {
        FocusedPanelIndex = (FocusedPanelIndex - 1 + _panelCount) % _panelCount;
    }

    public void NavigateTo(TuiScreen screen, int panelCount = 6)
    {
        _history.Push((CurrentScreen, _panelCount));
        CurrentScreen = screen;
        _panelCount = panelCount;
        FocusedPanelIndex = 0;
    }

    public void NavigateBack()
    {
        if (_history.Count == 0) return;
        var previous = _history.Pop();
        CurrentScreen = previous.Screen;
        _panelCount = previous.PanelCount;
        FocusedPanelIndex = 0;
    }

    public bool IsPanelFocused(int panelIndex) => FocusedPanelIndex == panelIndex;
}
