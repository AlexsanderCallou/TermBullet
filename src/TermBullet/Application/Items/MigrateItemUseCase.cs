using TermBullet.Application.Ports;

namespace TermBullet.Application.Items;

public sealed class MigrateItemUseCase(
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

        item.MarkMigrated(clock.UtcNow);

        await itemRepository.UpdateAsync(item, cancellationToken);

        return ItemResult.From(item);
    }
}
