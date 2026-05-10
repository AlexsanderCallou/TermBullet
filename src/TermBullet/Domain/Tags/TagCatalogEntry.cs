namespace TermBullet.Domain.Tags;

public sealed class TagCatalogEntry
{
    private TagCatalogEntry(
        string name,
        string? description,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        Name = name;
        Description = description;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static TagCatalogEntry Create(string name, string? description, DateTimeOffset createdAt) =>
        new(NormalizeName(name), NormalizeDescription(description), createdAt, createdAt);

    public static TagCatalogEntry Restore(
        string name,
        string? description,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        if (updatedAt < createdAt)
        {
            throw new ArgumentOutOfRangeException(nameof(updatedAt), "UpdatedAt cannot be before CreatedAt.");
        }

        return new TagCatalogEntry(
            NormalizeName(name),
            NormalizeDescription(description),
            createdAt,
            updatedAt);
    }

    public void Rename(string name, DateTimeOffset changedAt)
    {
        Name = NormalizeName(name);
        UpdatedAt = changedAt;
    }

    public void EditDescription(string? description, DateTimeOffset changedAt)
    {
        Description = NormalizeDescription(description);
        UpdatedAt = changedAt;
    }

    private static string NormalizeName(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return value.Trim().ToLowerInvariant();
    }

    private static string? NormalizeDescription(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
