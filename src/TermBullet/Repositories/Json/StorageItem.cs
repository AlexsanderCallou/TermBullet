using System.Text.Json.Serialization;

namespace TermBullet.Repositories.Json;

internal sealed class StorageItem
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("public_ref")]
    public string PublicRef { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = "open";

    [JsonPropertyName("collection")]
    public string Collection { get; set; } = "today";

    [JsonPropertyName("priority")]
    public string Priority { get; set; } = "none";

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = [];

    [JsonPropertyName("scheduled_at")]
    public DateTimeOffset? ScheduledAt { get; set; }

    [JsonPropertyName("version")]
    public int Version { get; set; } = 1;

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }

    [JsonPropertyName("completed_at")]
    public DateTimeOffset? CompletedAt { get; set; }

    [JsonPropertyName("cancelled_at")]
    public DateTimeOffset? CancelledAt { get; set; }
}
