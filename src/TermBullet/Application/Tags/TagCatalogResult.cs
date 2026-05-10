using TermBullet.Domain.Tags;

namespace TermBullet.Application.Tags;

public sealed record TagCatalogResult(
    string Name,
    string? Description,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static TagCatalogResult From(TagCatalogEntry tag) =>
        new(tag.Name, tag.Description, tag.CreatedAt, tag.UpdatedAt);
}
