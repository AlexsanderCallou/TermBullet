namespace TermBullet.Services.Ai;

public sealed record AiPlanningProviderResponse(
    string Content,
    string? Model = null);
