using TermBullet.Core.Items;

namespace TermBullet.Application.Ports;

public interface IItemArchiveReader
{
    Task<IReadOnlyCollection<Item>> ListAllAsync(CancellationToken cancellationToken = default);
}
