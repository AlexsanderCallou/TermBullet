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
    public void IsPromptTextInput_treats_plain_letters_as_text(char key)
    {
        Assert.True(PlanningShortcutPolicy.IsPromptTextInput(new KeyEvent((Key)key, default)));
    }

    [Theory]
    [InlineData(Key.Enter)]
    [InlineData(Key.Esc)]
    [InlineData(Key.Tab)]
    [InlineData(Key.BackTab)]
    public void IsPromptTextInput_keeps_control_keys_available(Key key)
    {
        Assert.False(PlanningShortcutPolicy.IsPromptTextInput(new KeyEvent(key, default)));
    }

    [Fact]
    public void IsPromptTextInput_keeps_help_key_available()
    {
        Assert.False(PlanningShortcutPolicy.IsPromptTextInput(new KeyEvent((Key)'?', default)));
    }
}
