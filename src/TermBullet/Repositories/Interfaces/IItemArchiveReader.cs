using TermBullet.Domain.Items;

namespace TermBullet.Repositories.Interfaces;

public interface IItemArchiveReader
{
    Task<IReadOnlyCollection<Item>> ListAllAsync(CancellationToken cancellationToken = default);
}
