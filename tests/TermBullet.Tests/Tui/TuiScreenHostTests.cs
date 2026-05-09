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

    [Fact]
    public void SanitizeListItems_replaces_empty_rows_with_safe_space()
    {
        var rows = TuiScreenUtilities.SanitizeListItems(["alpha", string.Empty, "   "]);

        Assert.Equal(["alpha", " ", " "], rows);
    }

    [Fact]
    public void SanitizeListItems_returns_safe_placeholder_for_empty_source()
    {
        var rows = TuiScreenUtilities.SanitizeListItems([]);

        Assert.Equal([" "], rows);
    }

    [Fact]
    public void TryHandleEnter_invokes_action_for_enter_key()
    {
        var invoked = false;

        var handled = TuiScreenUtilities.TryHandleEnter(Key.Enter, () => invoked = true);

        Assert.True(handled);
        Assert.True(invoked);
    }

    [Fact]
    public void TryHandleEnter_ignores_non_enter_key()
    {
        var invoked = false;

        var handled = TuiScreenUtilities.TryHandleEnter(Key.Tab, () => invoked = true);

        Assert.False(handled);
        Assert.False(invoked);
    }
}
