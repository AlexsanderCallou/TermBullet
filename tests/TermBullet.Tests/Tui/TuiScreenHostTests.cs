using Terminal.Gui;
using TermBullet.Tui;

namespace TermBullet.Tests.Tui;

public sealed class TuiScreenHostTests
{
    [Fact]
    public void ReplaceContent_removes_previous_screen_root()
    {
        var top = new Toplevel();
        var host = new TuiScreenHost(top);

        var first = host.ReplaceContent();
        first.Add(new Label("first"));

        var second = host.ReplaceContent();
        second.Add(new Label("second"));

        Assert.DoesNotContain(first, top.Subviews);
        Assert.Contains(second, top.Subviews);
        Assert.Single(top.Subviews);
    }
}
