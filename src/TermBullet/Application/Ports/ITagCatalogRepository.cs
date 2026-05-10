using TermBullet.Core.Tags;

namespace TermBullet.Application.Ports;

public interface ITagCatalogRepository
{
    Task<IReadOnlyCollection<TagCatalogEntry>> ListAsync(CancellationToken cancellationToken = default);

    Task<TagCatalogEntry?> FindByNameAsync(string name, CancellationToken cancellationToken = default);

    Task AddAsync(TagCatalogEntry tag, CancellationToken cancellationToken = default);
}
