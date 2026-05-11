using TermBullet.Repositories.Interfaces;
using TermBullet.Domain.Items;

namespace TermBullet.Application.Items;

public sealed class GetMonthItemsUseCase(IItemRepository itemRepository)
{
    public async Task<IReadOnlyCollection<ItemResult>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var items = await itemRepository.ListAsync(
            collection: ItemCollection.Month,
            cancellationToken: cancellationToken);

        return items.Select(ItemResult.From).ToArray();
    }
}
