namespace TermBullet.Application.Ai;

public sealed record GenerateAiPlanningResponseResult(
    AiPlanningDraft? Draft,
    string? AssistantMessage,
    string ProviderModel,
    AiPlanningModelRequest ModelRequest)
{
    public bool HasDraft => Draft is not null;
}
