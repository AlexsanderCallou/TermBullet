using TermBullet.Core.Items;

namespace TermBullet.Application.Items;

public sealed record CreateItemResult(
    Guid Id,
    string PublicRef,
    ItemType Type,
    ItemStatus Status,
    ItemCollection Collection,
    Priority Priority,
    int Version,
    DateTimeOffset CreatedAt)
{
    public static CreateItemResult From(Item item)
    {
        return new CreateItemResult(
            item.Id,
            item.PublicRef.Value,
            item.Type,
            item.Status,
            item.Collection,
            item.Priority,
            item.Version,
            item.CreatedAt);
    }
}
