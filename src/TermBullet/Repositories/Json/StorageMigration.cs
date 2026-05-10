using System.Text.Json.Serialization;

namespace TermBullet.Repositories.Json;

internal sealed class StorageMigration
{
    [JsonPropertyName("from_period")]
    public string FromPeriod { get; set; } = string.Empty;

    [JsonPropertyName("to_period")]
    public string ToPeriod { get; set; } = string.Empty;

    [JsonPropertyName("migrated_at")]
    public DateTimeOffset MigratedAt { get; set; }

    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;
}
