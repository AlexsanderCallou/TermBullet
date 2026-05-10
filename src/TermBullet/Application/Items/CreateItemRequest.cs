using TermBullet.Domain.Items;

namespace TermBullet.Application.Items;

public sealed class CreateItemRequest
{
    public required ItemType Type { get; init; }

    public required string Content { get; init; }

    public ItemCollection Collection { get; init; } = ItemCollection.Today;

    public Priority Priority { get; init; } = Priority.None;

    public string? Description { get; init; }

    public IReadOnlyCollection<string>? Tags { get; init; }

    public DateOnly? PlannedFor { get; init; }

    public DateTimeOffset? ScheduledAt { get; init; }
}
