using TermBullet.Application.Ports;
using TermBullet.Core.Tags;

namespace TermBullet.Application.Tags;

public sealed class CreateTagUseCase(
    ITagCatalogRepository tagCatalogRepository,
    IClock clock)
{
    public async Task<TagCatalogResult> ExecuteAsync(
        CreateTagRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var tag = TagCatalogEntry.Create(request.Name, request.Description, clock.UtcNow);
        var existing = await tagCatalogRepository.FindByNameAsync(tag.Name, cancellationToken);
        if (existing is not null)
        {
            throw new InvalidOperationException($"Tag already exists: {tag.Name}.");
        }

        await tagCatalogRepository.AddAsync(tag, cancellationToken);
        return TagCatalogResult.From(tag);
    }
}
