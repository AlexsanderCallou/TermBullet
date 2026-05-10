using TermBullet.Domain.Tags;

namespace TermBullet.Repositories.Interfaces;

public interface ITagCatalogRepository
{
    Task<IReadOnlyCollection<TagCatalogEntry>> ListAsync(CancellationToken cancellationToken = default);

    Task<TagCatalogEntry?> FindByNameAsync(string name, CancellationToken cancellationToken = default);

    Task AddAsync(TagCatalogEntry tag, CancellationToken cancellationToken = default);
}
