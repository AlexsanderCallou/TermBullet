using TermBullet.Domain.Items;

namespace TermBullet.Application.Items;

public sealed class EditItemRequest
{
    public required string PublicRef { get; init; }

    public required string Content { get; init; }

    public string? Description { get; init; }

    public ItemCollection? Collection { get; init; }

    public Priority? Priority { get; init; }

    public IReadOnlyCollection<string>? Tags { get; init; }

    public DateTimeOffset? ScheduledAt { get; init; }
}
