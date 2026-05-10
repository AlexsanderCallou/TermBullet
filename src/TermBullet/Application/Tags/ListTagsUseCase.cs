using TermBullet.Repositories.Interfaces;

namespace TermBullet.Application.Tags;

public sealed class ListTagsUseCase(ITagCatalogRepository tagCatalogRepository)
{
    public async Task<IReadOnlyCollection<TagCatalogResult>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var tags = await tagCatalogRepository.ListAsync(cancellationToken);
        return tags
            .OrderBy(tag => tag.Name, StringComparer.OrdinalIgnoreCase)
            .Select(TagCatalogResult.From)
            .ToArray();
    }
}
