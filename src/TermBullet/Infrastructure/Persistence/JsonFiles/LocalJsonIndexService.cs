using System.Text.Json;
using System.Text.Json.Serialization;

namespace TermBullet.Infrastructure.Persistence.JsonFiles;

public sealed class LocalJsonIndexService(
    string projectRootPath,
    SafeJsonFileStore? fileStore = null)
{
    private readonly SafeJsonFileStore _fileStore = fileStore ?? new SafeJsonFileStore();

    public async Task RebuildAsync(CancellationToken cancellationToken = default)
    {
        var dataRoot = Path.Combine(projectRootPath, "data");
        Directory.CreateDirectory(dataRoot);

        var monthlyFiles = Directory.Exists(dataRoot)
            ? Directory.EnumerateFiles(dataRoot, "data_??_????.json", SearchOption.AllDirectories)
            : [];

        var items = new List<IndexItem>();
        foreach (var file in monthlyFiles)
        {
            if (Path.GetFileName(file).Equals("index.json", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var json = await File.ReadAllTextAsync(file, cancellationToken);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var period = root.TryGetProperty("period", out var periodElement)
                ? periodElement.GetString() ?? string.Empty
                : string.Empty;

            if (!root.TryGetProperty("items", out var itemsElement) || itemsElement.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var item in itemsElement.EnumerateArray())
            {
                items.Add(new IndexItem
                {
                    Id = item.GetProperty("id").GetGuid(),
                    PublicRef = item.GetProperty("public_ref").GetString() ?? string.Empty,
                    Type = item.GetProperty("type").GetString() ?? string.Empty,
                    Status = item.GetProperty("status").GetString() ?? string.Empty,
                    Collection = item.GetProperty("collection").GetString() ?? string.Empty,
                    Priority = item.GetProperty("priority").GetString() ?? string.Empty,
                    Content = item.GetProperty("content").GetString() ?? string.Empty,
                    Tags = item.TryGetProperty("tags", out var tags) && tags.ValueKind == JsonValueKind.Array
                        ? tags.EnumerateArray().Select(tag => tag.GetString() ?? string.Empty).ToArray()
                        : [],
                    Period = period,
                    SourceFile = GetRelativeDataPath(file, projectRootPath),
                    UpdatedAt = item.GetProperty("updated_at").GetDateTimeOffset()
                });
            }
        }

        var index = new LocalIndexDocument
        {
            GeneratedAt = DateTimeOffset.UtcNow,
            Items = items.OrderBy(item => item.PublicRef, StringComparer.Ordinal).ToList()
        };

        var indexPath = Path.Combine(dataRoot, "index.json");
        var indexBackupPath = Path.Combine(dataRoot, "index.backup.json");
        var output = JsonSerializer.Serialize(index, JsonOptions);
        await _fileStore.WriteAsync(indexPath, indexBackupPath, output, cancellationToken);
    }

    private static string GetRelativeDataPath(string absolutePath, string rootPath)
    {
        var relative = Path.GetRelativePath(rootPath, absolutePath);
        return relative.Replace('\\', '/');
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private sealed class LocalIndexDocument
    {
        [JsonPropertyName("generated_at")]
        public DateTimeOffset GeneratedAt { get; set; }

        [JsonPropertyName("items")]
        public List<IndexItem> Items { get; set; } = [];
    }

    private sealed class IndexItem
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("public_ref")]
        public string PublicRef { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("collection")]
        public string Collection { get; set; } = string.Empty;

        [JsonPropertyName("priority")]
        public string Priority { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("tags")]
        public string[] Tags { get; set; } = [];

        [JsonPropertyName("period")]
        public string Period { get; set; } = string.Empty;

        [JsonPropertyName("source_file")]
        public string SourceFile { get; set; } = string.Empty;

        [JsonPropertyName("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
