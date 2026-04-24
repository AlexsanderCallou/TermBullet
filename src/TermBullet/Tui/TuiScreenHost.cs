using Terminal.Gui;

namespace TermBullet.Tui;

public sealed class TuiScreenHost(Toplevel top)
{
    private View? _currentRoot;

    public View ReplaceContent()
    {
        if (_currentRoot is not null)
        {
            top.Remove(_currentRoot);
        }

        _currentRoot = new View
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        top.Add(_currentRoot);

        return _currentRoot;
    }
}
