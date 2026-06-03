namespace TermBullet.Application.Ai;

public sealed record AiPlanningModelRequest(
    AiPlanningMode Mode,
    string? Tag,
    IReadOnlyList<AiPlanningMessage> Messages,
    IReadOnlyList<AiPlanningContextItem> ContextItems,
    bool RequireStructuredDraft = true,
    int? MaxOutputTokens = null);
