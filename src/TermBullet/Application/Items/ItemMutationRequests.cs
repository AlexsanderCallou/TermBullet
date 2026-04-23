using TermBullet.Core.Items;

namespace TermBullet.Application.Items;

public sealed class EditItemRequest
{
    public required string PublicRef { get; init; }

    public required string Content { get; init; }

    public string? Description { get; init; }
}

public sealed class MoveItemRequest
{
    public required string PublicRef { get; init; }

    public required ItemCollection Collection { get; init; }
}

public sealed class SetItemPriorityRequest
{
    public required string PublicRef { get; init; }

    public required Priority Priority { get; init; }
}

public sealed class TagItemRequest
{
    public required string PublicRef { get; init; }

    public required string Tag { get; init; }
}

public sealed class UntagItemRequest
{
    public required string PublicRef { get; init; }

    public required string Tag { get; init; }
}
