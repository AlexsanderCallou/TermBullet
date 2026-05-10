using System.Text.Json;
using System.Text.Json.Serialization;

namespace TermBullet.Repositories.Json;

internal sealed class HistoryEntry
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("item_id")]
    public Guid ItemId { get; set; }

    [JsonPropertyName("public_ref")]
    public string PublicRef { get; set; } = string.Empty;

    [JsonPropertyName("event_type")]
    public string EventType { get; set; } = string.Empty;

    [JsonPropertyName("occurred_at")]
    public DateTimeOffset OccurredAt { get; set; }

    [JsonPropertyName("data")]
    public JsonElement Data { get; set; }
}
