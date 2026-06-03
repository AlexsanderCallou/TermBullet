using System.Text.Json.Serialization;

namespace TermBullet.Application.Ai;

public sealed class AiPlanningResponseEnvelope
{
    [JsonPropertyName("kind")]
    public string? Kind { get; init; }

    [JsonPropertyName("message")]
    public string? Message { get; init; }

    [JsonPropertyName("draft_ready")]
    public bool DraftReady { get; init; }

    [JsonPropertyName("draft")]
    public AiPlanningDraft? Draft { get; init; }
}
