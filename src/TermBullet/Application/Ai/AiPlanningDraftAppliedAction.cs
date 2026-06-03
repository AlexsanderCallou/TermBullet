namespace TermBullet.Application.Ai;

public sealed record AiPlanningDraftAppliedAction(
    string Type,
    string? PublicRef = null,
    string? Tag = null,
    string? Collection = null);
