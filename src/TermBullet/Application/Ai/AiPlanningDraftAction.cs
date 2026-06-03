using System.Text.Json.Serialization;

namespace TermBullet.Application.Ai;

public sealed class AiPlanningDraftAction
{
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("public_ref")]
    public string? PublicRef { get; init; }

    [JsonPropertyName("tag")]
    public string? Tag { get; init; }

    [JsonPropertyName("collection")]
    public string? Collection { get; init; }

    [JsonPropertyName("content")]
    public string? Content { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("priority")]
    public string? Priority { get; init; }
}
