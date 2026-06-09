using Terminal.Gui;
using TermBullet.Tui.Screens;

namespace TermBullet.Tests.Tui;

public sealed class PlanningShortcutPolicyTests
{
    [Theory]
    [InlineData('a')]
    [InlineData('d')]
    [InlineData('e')]
    [InlineData('q')]
    [InlineData('1')]
    [InlineData('?')]
    public void IsPromptTextInput_treats_plain_letters_as_text(char key)
    {
        Assert.True(PlanningShortcutPolicy.IsPromptTextInput(new KeyEvent((Key)key, default)));
    }

    [Theory]
    [InlineData(Key.Esc)]
    [InlineData(Key.Tab)]
    [InlineData(Key.BackTab)]
    public void IsPromptTextInput_keeps_control_keys_available(Key key)
    {
        Assert.False(PlanningShortcutPolicy.IsPromptTextInput(new KeyEvent(key, default)));
    }

    [Fact]
    public void IsPromptTextInput_keeps_enter_owned_by_text_input()
    {
        Assert.True(PlanningShortcutPolicy.IsPromptTextInput(new KeyEvent(Key.Enter, default)));
    }
}
