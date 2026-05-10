using TermBullet.Application.Ports;
using TermBullet.Core.Items;
using TermBullet.Core.Refs;

namespace TermBullet.Application.Items;

public sealed class CreateItemUseCase(
    IItemRepository itemRepository,
    IClock clock,
    IIdGenerator idGenerator)
{
    public async Task<CreateItemResult> ExecuteAsync(
        CreateItemRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var now = clock.UtcNow;
        var currentSequence = await itemRepository.GetCurrentPublicRefSequenceAsync(
            request.Type,
            now.Month,
            now.Year,
            cancellationToken);

        var publicRef = PublicRefGenerator.Next(
            request.Type,
            now.Month,
            now.Year,
            currentSequence);

        if (await itemRepository.PublicRefExistsAsync(publicRef.Value, cancellationToken))
        {
            throw new DuplicatePublicRefException(publicRef.Value);
        }

        var item = Item.Create(
            idGenerator.NewId(),
            publicRef,
            request.Type,
            request.Content,
            request.Collection,
            now,
            request.Description,
            ResolvePriority(request),
            request.Tags,
            ResolvePlannedFor(request, now),
            request.ScheduledAt);

        await itemRepository.AddAsync(item, cancellationToken);

        return CreateItemResult.From(item);
    }

    private static DateOnly? ResolvePlannedFor(CreateItemRequest request, DateTimeOffset now)
    {
        if (request.Type != ItemType.Task)
        {
            return null;
        }

        if (request.Collection == ItemCollection.Backlog)
        {
            return request.PlannedFor;
        }

        return request.PlannedFor ?? DateOnly.FromDateTime(now.UtcDateTime);
    }

    private static Priority ResolvePriority(CreateItemRequest request) =>
        request.Type == ItemType.Task ? request.Priority : Priority.None;
}
