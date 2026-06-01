namespace TermBullet.Repositories.Interfaces;

public sealed record ItemHistoryEntry(
    Guid Id,
    Guid ItemId,
    string PublicRef,
    string EventType,
    DateTimeOffset OccurredAt,
    string DataJson);
