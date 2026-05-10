using TermBullet.Repositories.Interfaces;
using TermBullet.Domain.Refs;

namespace TermBullet.Application.Items;

public sealed class DeleteItemUseCase(IItemRepository itemRepository)
{
    public async Task ExecuteAsync(
        string publicRef,
        CancellationToken cancellationToken = default)
    {
        var parsedPublicRef = PublicRef.Parse(publicRef);

        try
        {
            await itemRepository.DeleteByPublicRefAsync(parsedPublicRef.Value, cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            throw new ItemNotFoundException(parsedPublicRef.Value);
        }
    }
}
