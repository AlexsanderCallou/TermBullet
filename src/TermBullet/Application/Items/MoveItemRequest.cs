using TermBullet.Domain.Items;

namespace TermBullet.Application.Items;

public sealed class MoveItemRequest
{
    public required string PublicRef { get; init; }

    public required ItemCollection Collection { get; init; }
}
