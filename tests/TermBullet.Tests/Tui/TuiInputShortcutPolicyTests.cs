using Terminal.Gui;
using TermBullet.Tui;

namespace TermBullet.Tests.Tui;

public sealed class TuiInputShortcutPolicyTests
{
    [Theory]
    [InlineData('e')]
    [InlineData('q')]
    [InlineData('1')]
    [InlineData('?')]
    public void IsTextInputOwnedKey_keeps_plain_action_keys_for_text_fields(char key)
    {
        Assert.True(TuiScreenUtilities.IsTextInputOwnedKey(new KeyEvent((Key)key, default)));
    }

    [Theory]
    [InlineData(Key.Esc)]
    [InlineData(Key.Tab)]
    [InlineData(Key.BackTab)]
    public void IsTextInputOwnedKey_leaves_navigation_keys_available(Key key)
    {
        Assert.False(TuiScreenUtilities.IsTextInputOwnedKey(new KeyEvent(key, default)));
    }
}
