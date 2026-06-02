using System.Text.Json;
using System.Text.Json.Serialization;
using TermBullet.Repositories.Interfaces;
using TermBullet.Domain.Items;
using TermBullet.Domain.Tags;

namespace TermBullet.Repositories.Json;

public sealed class JsonTagCatalogRepository(
    string projectRootPath,
    JsonFileStore fileStore) : ITagCatalogRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public string TagsPath => GetTagsPath();

    public async Task<IReadOnlyCollection<TagCatalogEntry>> ListAsync(CancellationToken cancellationToken = default)
    {
        var document = await ReadAsync(cancellationToken);
        EnsureDefaultTag(document);
        await WriteAsync(document, cancellationToken);
        return document.Tags
            .Select(tag => TagCatalogEntry.Restore(tag.Name, tag.Description, tag.CreatedAt, tag.UpdatedAt))
            .OrderBy(tag => tag.CreatedAt)
            .ThenBy(tag => tag.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public async Task<TagCatalogEntry?> FindByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = TagCatalogEntry.Create(name, null, DateTimeOffset.UnixEpoch).Name;
        var document = await ReadAsync(cancellationToken);
        EnsureDefaultTag(document);
        var tag = document.Tags.FirstOrDefault(existing =>
            string.Equals(existing.Name, normalizedName, StringComparison.OrdinalIgnoreCase));

        return tag is null
            ? null
            : TagCatalogEntry.Restore(tag.Name, tag.Description, tag.CreatedAt, tag.UpdatedAt);
    }

    public async Task AddAsync(TagCatalogEntry tag, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tag);

        var document = await ReadAsync(cancellationToken);
        EnsureDefaultTag(document);
        if (document.Tags.Any(existing => string.Equals(existing.Name, tag.Name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Tag already exists: {tag.Name}.");
        }

        document.Tags.Add(ToStorageTag(tag));
        await WriteAsync(document, cancellationToken);
    }

    private static void EnsureDefaultTag(TagCatalogDocument document)
    {
        if (document.Tags.Any(tag => string.Equals(tag.Name, Item.DefaultTag, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        document.Tags.Add(new StorageTag
        {
            Name = Item.DefaultTag,
            Description = "Default capture tag",
            CreatedAt = DateTimeOffset.UnixEpoch,
            UpdatedAt = DateTimeOffset.UnixEpoch
        });
    }

    private async Task<TagCatalogDocument> ReadAsync(CancellationToken cancellationToken)
    {
        var tagsPath = GetTagsPath();
        if (!File.Exists(tagsPath))
        {
            return new TagCatalogDocument();
        }

        var json = await fileStore.ReadOrRecoverAsync(tagsPath, GetBackupPath(), cancellationToken);
        var document = JsonSerializer.Deserialize<TagCatalogDocument>(json, JsonOptions)
            ?? new TagCatalogDocument();
        document.Tags ??= [];
        return document;
    }

    private async Task WriteAsync(TagCatalogDocument document, CancellationToken cancellationToken)
    {
        document.Tags = document.Tags
            .OrderBy(tag => tag.CreatedAt)
            .ThenBy(tag => tag.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var json = JsonSerializer.Serialize(document, JsonOptions);
        await fileStore.WriteAsync(GetTagsPath(), GetBackupPath(), json, cancellationToken);
    }

    private string GetTagsPath() => Path.Combine(projectRootPath, "data", "tags.json");

    private string GetBackupPath() => Path.Combine(projectRootPath, "data", "tags.backup.json");

    private static StorageTag ToStorageTag(TagCatalogEntry tag) =>
        new()
        {
            Name = tag.Name,
            Description = tag.Description,
            CreatedAt = tag.CreatedAt,
            UpdatedAt = tag.UpdatedAt
        };

    private sealed class TagCatalogDocument
    {
        [JsonPropertyName("tags")]
        public List<StorageTag> Tags { get; set; } = [];
    }

    private sealed class StorageTag
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
