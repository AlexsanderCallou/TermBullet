namespace TermBullet.Application.Ai;

public sealed record AiPlanningMessage(
    AiPlanningMessageRole Role,
    string Content);
