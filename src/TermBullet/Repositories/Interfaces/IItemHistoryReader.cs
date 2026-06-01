namespace TermBullet.Repositories.Interfaces;

public interface IItemHistoryReader
{
    Task<IReadOnlyCollection<ItemHistoryEntry>> ListHistoryByPublicRefAsync(
        string publicRef,
        CancellationToken cancellationToken = default);
}
