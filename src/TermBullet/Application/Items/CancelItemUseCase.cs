using TermBullet.Services.Clock;
using TermBullet.Repositories.Interfaces;

namespace TermBullet.Application.Items;

public sealed class CancelItemUseCase(
    IItemRepository itemRepository,
    IClock clock)
{
    public async Task<ItemResult> ExecuteAsync(
        string publicRef,
        CancellationToken cancellationToken = default)
    {
        var item = await ItemLookup.FindRequiredAsync(
            itemRepository,
            publicRef,
            cancellationToken);

        item.Cancel(clock.UtcNow);

        await itemRepository.UpdateAsync(item, cancellationToken);

        return ItemResult.From(item);
    }
}
