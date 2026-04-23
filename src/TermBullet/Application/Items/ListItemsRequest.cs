using TermBullet.Core.Items;

namespace TermBullet.Application.Items;

public sealed class ListItemsRequest
{
    public ItemCollection? Collection { get; init; }

    public ItemStatus? Status { get; init; }
}
