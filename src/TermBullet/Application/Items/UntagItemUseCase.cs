using TermBullet.Application.Ports;

namespace TermBullet.Application.Items;

public sealed class UntagItemUseCase(
    IItemRepository itemRepository,
    IClock clock)
{
    public async Task<ItemResult> ExecuteAsync(
        UntagItemRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var item = await ItemLookup.FindRequiredAsync(
            itemRepository,
            request.PublicRef,
            cancellationToken);

        item.RemoveTag(request.Tag, clock.UtcNow);

        await itemRepository.UpdateAsync(item, cancellationToken);

        return ItemResult.From(item);
    }
}
