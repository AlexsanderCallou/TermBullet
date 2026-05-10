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
                DestinationCollection = ItemCollection.Today,
                PlannedFor = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime)
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

        var destinationCollection = ResolveDestinationCollection(request);
        var plannedFor = ResolvePlannedFor(request, destinationCollection);
        var now = clock.UtcNow;
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
            item.Tags,
            plannedFor);

        await itemRepository.UpdateAsync(item, cancellationToken);
        await itemRepository.AddAsync(migratedItem, cancellationToken);

        return ItemResult.From(item);
    }

    private static ItemCollection ResolveDestinationCollection(MigrateItemRequest request)
    {
        return request.DestinationCollection switch
        {
            ItemCollection.Backlog => ItemCollection.Backlog,
            ItemCollection.Today or ItemCollection.Week => ItemCollection.Week,
            _ => throw new ArgumentOutOfRangeException(
                nameof(request.DestinationCollection),
                request.DestinationCollection,
                "Unsupported migration destination.")
        };
    }

    private static DateOnly? ResolvePlannedFor(
        MigrateItemRequest request,
        ItemCollection destinationCollection)
    {
        if (destinationCollection == ItemCollection.Backlog)
        {
            return null;
        }

        return request.PlannedFor
            ?? throw new ArgumentException("Date migration requires planned_for.", nameof(request));
    }
}
