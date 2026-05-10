using TermBullet.Services.Clock;
using TermBullet.Repositories.Interfaces;
using TermBullet.Domain.Items;

namespace TermBullet.Application.Items;

public sealed class MigrateItemUseCase(
    IItemRepository itemRepository,
    IClock clock)
{
    public async Task<ItemResult> ExecuteAsync(
        string publicRef,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(
            new MigrateItemRequest
            {
                PublicRef = publicRef,
                DestinationCollection = ItemCollection.Today
            },
            cancellationToken);
    }

    public async Task<ItemResult> ExecuteAsync(
        MigrateItemRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var item = await ItemLookup.FindRequiredAsync(
            itemRepository,
            request.PublicRef,
            cancellationToken);

        if (item.Type != ItemType.Task)
        {
            throw new InvalidOperationException("Only tasks can be migrated.");
        }

        var now = clock.UtcNow;
        var destinationCollection = ResolveDestinationCollection(request);
        item.MoveTo(destinationCollection, now);
        await itemRepository.UpdateAsync(item, cancellationToken);

        return ItemResult.From(item);
    }

    private static ItemCollection ResolveDestinationCollection(MigrateItemRequest request)
    {
        return request.DestinationCollection switch
        {
            ItemCollection.Today => ItemCollection.Today,
            ItemCollection.Week => ItemCollection.Week,
            ItemCollection.Month => ItemCollection.Month,
            ItemCollection.Backlog => ItemCollection.Backlog,
            _ => throw new ArgumentOutOfRangeException(
                nameof(request.DestinationCollection),
                request.DestinationCollection,
                "Unsupported migration destination.")
        };
    }
}
