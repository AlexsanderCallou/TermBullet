namespace TermBullet.Repositories.Interfaces;

public interface IItemHistoryWriter
{
    Task AppendHistoryAsync(
        Guid itemId,
        string publicRef,
        string eventType,
        object? data = null,
        CancellationToken cancellationToken = default);
}
