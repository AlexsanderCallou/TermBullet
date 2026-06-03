namespace TermBullet.Application.Ai;

public sealed record GenerateAiPlanningDraftResult(
    AiPlanningDraft Draft,
    string? ProviderModel,
    AiPlanningModelRequest ModelRequest);
