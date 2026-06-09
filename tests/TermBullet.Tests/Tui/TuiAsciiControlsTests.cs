using TermBullet.Tui;

namespace TermBullet.Tests.Tui;

public sealed class TuiAsciiControlsTests
{
    [Fact]
    public void Checkbox_uses_spaced_ascii_standard()
    {
        Assert.Equal("[ x ]", TuiAsciiControls.Checkbox(true));
        Assert.Equal("[   ]", TuiAsciiControls.Checkbox(false));
    }

    [Fact]
    public void Radio_uses_spaced_ascii_standard()
    {
        Assert.Equal("( x )", TuiAsciiControls.Radio(true));
        Assert.Equal("(   )", TuiAsciiControls.Radio(false));
    }

    [Fact]
    public void Lines_prefix_labels_with_standard_markers()
    {
        Assert.Equal("[ x ] Done", TuiAsciiControls.CheckboxLine(true, "Done"));
        Assert.Equal("(   ) Month", TuiAsciiControls.RadioLine(false, "Month"));
        Assert.Equal("> Save", TuiAsciiControls.ActionLine(true, "Save"));
    }
}
