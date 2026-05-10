using TermBullet.Repositories.Interfaces;
using TermBullet.Domain.Items;
using TermBullet.Domain.Refs;

namespace TermBullet.Application.Items;

internal static class ItemLookup
{
    public static async Task<Item> FindRequiredAsync(
        IItemRepository itemRepository,
        string publicRef,
        CancellationToken cancellationToken)
    {
        var parsedPublicRef = PublicRef.Parse(publicRef);
        var item = await itemRepository.FindByPublicRefAsync(
            parsedPublicRef.Value,
            cancellationToken);

        if (item is null)
        {
            throw new ItemNotFoundException(parsedPublicRef.Value);
        }

        return item;
    }
}
