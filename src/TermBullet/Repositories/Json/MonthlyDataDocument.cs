using System.Text.Json.Serialization;

namespace TermBullet.Repositories.Json;

internal sealed class MonthlyDataDocument
{
    [JsonPropertyName("period")]
    public string? Period { get; set; }

    [JsonPropertyName("file_name")]
    public string? FileName { get; set; }

    [JsonPropertyName("public_ref_sequences")]
    public Dictionary<string, int> PublicRefSequences { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    [JsonPropertyName("items")]
    public List<StorageItem> Items { get; set; } = [];

    [JsonPropertyName("history")]
    public List<HistoryEntry> History { get; set; } = [];

    public static MonthlyDataDocument CreateEmpty(int year, int month)
    {
        return new MonthlyDataDocument
        {
            Period = $"{year:0000}-{month:00}",
            FileName = $"data_{month:00}_{year:0000}.json",
            PublicRefSequences = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["task"] = 0,
                ["note"] = 0,
                ["event"] = 0
            }
        };
    }
}
