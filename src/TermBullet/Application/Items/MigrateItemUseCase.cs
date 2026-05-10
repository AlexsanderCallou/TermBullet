using TermBullet.Services.Clock;
using TermBullet.Services.Ids;
using TermBullet.Repositories.Interfaces;
using TermBullet.Domain.Items;
using TermBullet.Domain.Refs;

namespace TermBullet.Application.Items;

public sealed class MigrateItemUseCase(
    IItemRepository itemRepository,
    IClock clock,
    IIdGenerator? idGenerator = null)
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
        var currentSequence = await itemRepository.GetCurrentPublicRefSequenceAsync(
            ItemType.Task,
            now.Month,
            now.Year,
            cancellationToken);
        var destinationRef = PublicRefGenerator.Next(
            ItemType.Task,
            now.Month,
            now.Year,
            currentSequence);

        if (await itemRepository.PublicRefExistsAsync(destinationRef.Value, cancellationToken))
        {
            throw new DuplicatePublicRefException(destinationRef.Value);
        }

        item.MarkMigrate(now);

        var migratedItem = Item.Create(
            idGenerator?.NewId() ?? Guid.NewGuid(),
            destinationRef,
            ItemType.Task,
            item.Content,
            destinationCollection,
            now,
            item.Description,
            item.Priority,
            item.Tags);

        await itemRepository.UpdateAsync(item, cancellationToken);
        await itemRepository.AddAsync(migratedItem, cancellationToken);

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
