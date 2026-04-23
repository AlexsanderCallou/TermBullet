using TermBullet.Application.Ports;

namespace TermBullet.Application.Items;

public sealed class ShowItemUseCase(IItemRepository itemRepository)
{
    public async Task<ItemResult> ExecuteAsync(
        string publicRef,
        CancellationToken cancellationToken = default)
    {
        var item = await ItemLookup.FindRequiredAsync(
            itemRepository,
            publicRef,
            cancellationToken);

        return ItemResult.From(item);
    }
}
