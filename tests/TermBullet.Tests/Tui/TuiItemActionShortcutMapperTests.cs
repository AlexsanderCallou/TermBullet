using Terminal.Gui;
using TermBullet.Tui;

namespace TermBullet.Tests.Tui;

public sealed class TuiItemActionShortcutMapperTests
{
    [Theory]
    [InlineData('x', TuiItemActionShortcut.Done)]
    [InlineData('z', TuiItemActionShortcut.Cancel)]
    [InlineData('>', TuiItemActionShortcut.Migrate)]
    [InlineData('d', TuiItemActionShortcut.Delete)]
    public void TryMap_returns_expected_action_for_lifecycle_shortcuts(
        char key,
        TuiItemActionShortcut expected)
    {
        var mapped = TuiItemActionShortcutMapper.TryMap((Key)key, out var action);

        Assert.True(mapped);
        Assert.Equal(expected, action);
    }

    [Fact]
    public void TryMap_returns_false_for_unmapped_key()
    {
        var mapped = TuiItemActionShortcutMapper.TryMap((Key)'r', out _);

        Assert.False(mapped);
    }
}
