using TermBullet.Application.Ports;
using TermBullet.Core.Items;
using TermBullet.Core.Refs;

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
