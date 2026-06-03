namespace TermBullet.Application.Ai;

public sealed class BuildAiPlanningRequest
{
    public AiPlanningMode Mode { get; init; }

    public string? Tag { get; init; }

    public string UserPrompt { get; init; } = string.Empty;

    public IReadOnlyList<AiPlanningMessage> ConversationHistory { get; init; } = [];

    public bool RequireStructuredDraft { get; init; } = true;
}
