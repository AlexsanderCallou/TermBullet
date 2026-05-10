using TermBullet.Repositories.Interfaces;
using TermBullet.Domain.Items;

namespace TermBullet.Application.Items;

public sealed class GetWeekItemsUseCase(IItemRepository itemRepository)
{
    public async Task<IReadOnlyCollection<ItemResult>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var items = await itemRepository.ListAsync(
            collection: ItemCollection.Week,
            cancellationToken: cancellationToken);

        return items.Select(ItemResult.From).ToArray();
    }
}
