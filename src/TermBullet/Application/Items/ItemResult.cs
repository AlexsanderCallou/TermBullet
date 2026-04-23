using TermBullet.Core.Items;

namespace TermBullet.Application.Items;

public sealed record ItemResult(
    Guid Id,
    string PublicRef,
    ItemType Type,
    string Content,
    string? Description,
    ItemStatus Status,
    ItemCollection Collection,
    Priority Priority,
    IReadOnlyCollection<string> Tags,
    int Version,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static ItemResult From(Item item)
    {
        return new ItemResult(
            item.Id,
            item.PublicRef.Value,
            item.Type,
            item.Content,
            item.Description,
            item.Status,
            item.Collection,
            item.Priority,
            item.Tags,
            item.Version,
            item.CreatedAt,
            item.UpdatedAt);
    }
}
