using TermBullet.Domain.Items;

namespace TermBullet.Application.Items;

public sealed class MigrateItemRequest
{
    public required string PublicRef { get; init; }

    public required ItemCollection DestinationCollection { get; init; }

    public DateOnly? PlannedFor { get; init; }
}
