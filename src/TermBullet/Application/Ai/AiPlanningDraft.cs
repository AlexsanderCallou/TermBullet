using System.Text.Json.Serialization;

namespace TermBullet.Application.Ai;

public sealed class AiPlanningDraft
{
    [JsonPropertyName("mode")]
    public string? Mode { get; init; }

    [JsonPropertyName("summary")]
    public string? Summary { get; init; }

    [JsonPropertyName("actions")]
    public IReadOnlyList<AiPlanningDraftAction> Actions { get; init; } = [];
}
