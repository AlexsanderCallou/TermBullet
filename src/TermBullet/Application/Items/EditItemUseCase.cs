using TermBullet.Application.Ports;

namespace TermBullet.Application.Items;

public sealed class EditItemUseCase(
    IItemRepository itemRepository,
    IClock clock)
{
    public async Task<ItemResult> ExecuteAsync(
        EditItemRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var item = await ItemLookup.FindRequiredAsync(
            itemRepository,
            request.PublicRef,
            cancellationToken);

        item.Edit(request.Content, clock.UtcNow, request.Description);

        await itemRepository.UpdateAsync(item, cancellationToken);

        return ItemResult.From(item);
    }
}
