using TermBullet.Domain.Items;

namespace TermBullet.Application.Items;

public sealed class SetItemPriorityRequest
{
    public required string PublicRef { get; init; }

    public required Priority Priority { get; init; }
}
