using TermBullet.Services.Ai;

namespace TermBullet.Application.Ai;

public sealed class GenerateAiPlanningDraftUseCase(
    BuildAiPlanningRequestUseCase buildRequestUseCase,
    IAiPlanningProvider provider,
    AiPlanningDraftValidator validator)
{
    public async Task<GenerateAiPlanningDraftResult> ExecuteAsync(
        BuildAiPlanningRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var strictRequest = new BuildAiPlanningRequest
        {
            Mode = request.Mode,
            Tag = request.Tag,
            UserPrompt = request.UserPrompt,
            ConversationHistory = request.ConversationHistory,
            RequireStructuredDraft = true
        };

        var result = await new GenerateAiPlanningResponseUseCase(
            buildRequestUseCase,
            provider,
            validator).ExecuteAsync(strictRequest, cancellationToken);
        if (result.Draft is null)
        {
            throw new InvalidOperationException("AI planning response did not include a structured draft.");
        }

        return new GenerateAiPlanningDraftResult(result.Draft, result.ProviderModel, result.ModelRequest);
    }
}
