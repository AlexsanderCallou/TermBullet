using TermBullet.Application.Ai;

namespace TermBullet.Tests.Application.Ai;

public sealed class AiPlanningDraftIntentTests
{
    [Theory]
    [InlineData("crie as tasks")]
    [InlineData("crie um roadmap para estudos da linguagem rust, tag estudos-rust")]
    [InlineData("ja adicione a tarefa inicial para hoje")]
    [InlineData("generate the project tasks")]
    public void RequiresStructuredDraft_returns_true_for_creation_prompts(string prompt)
    {
        Assert.True(AiPlanningDraftIntent.RequiresStructuredDraft(prompt));
    }

    [Theory]
    [InlineData("me ajude a pensar sobre estudos de rust")]
    [InlineData("quais topicos fariam sentido?")]
    [InlineData("tell me more about ownership")]
    public void RequiresStructuredDraft_returns_false_for_conversational_prompts(string prompt)
    {
        Assert.False(AiPlanningDraftIntent.RequiresStructuredDraft(prompt));
    }
}
