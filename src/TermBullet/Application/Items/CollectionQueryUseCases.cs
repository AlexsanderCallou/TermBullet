using TermBullet.Application.Ports;
using TermBullet.Core.Items;

namespace TermBullet.Application.Items;

public sealed class GetTodayItemsUseCase(IItemRepository itemRepository)
{
    public Task<IReadOnlyCollection<ItemResult>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        return CollectionQueryUseCaseShared.GetByCollectionAsync(
            itemRepository,
            ItemCollection.Today,
            cancellationToken);
    }
}

public sealed class GetWeekItemsUseCase(IItemRepository itemRepository)
{
    public Task<IReadOnlyCollection<ItemResult>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        return CollectionQueryUseCaseShared.GetByCollectionAsync(
            itemRepository,
            ItemCollection.Week,
            cancellationToken);
    }
}

public sealed class GetBacklogItemsUseCase(IItemRepository itemRepository)
{
    public Task<IReadOnlyCollection<ItemResult>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        return CollectionQueryUseCaseShared.GetByCollectionAsync(
            itemRepository,
            ItemCollection.Backlog,
            cancellationToken);
    }
}

internal static class CollectionQueryUseCaseShared
{
    public static async Task<IReadOnlyCollection<ItemResult>> GetByCollectionAsync(
        IItemRepository itemRepository,
        ItemCollection collection,
        CancellationToken cancellationToken)
    {
        var items = await itemRepository.ListAsync(
            collection: collection,
            cancellationToken: cancellationToken);

        return items.Select(ItemResult.From).ToArray();
    }
}
