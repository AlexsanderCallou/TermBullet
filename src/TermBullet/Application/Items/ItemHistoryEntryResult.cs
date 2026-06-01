namespace TermBullet.Application.Items;

public sealed record ItemHistoryEntryResult(
    DateTimeOffset OccurredAt,
    string EventType,
    string Summary);
