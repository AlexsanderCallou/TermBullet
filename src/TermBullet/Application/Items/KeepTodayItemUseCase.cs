using TermBullet.Domain.Items;
using TermBullet.Repositories.Interfaces;

namespace TermBullet.Application.Items;

public sealed class KeepTodayItemUseCase(
    IItemRepository itemRepository,
    IItemHistoryWriter historyWriter)
{
    public async Task<ItemResult> ExecuteAsync(
        string publicRef,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(publicRef);

        var item = await itemRepository.FindByPublicRefAsync(publicRef, cancellationToken)
            ?? throw new ItemNotFoundException(publicRef);

        if (item.Type != ItemType.Task)
        {
            throw new InvalidOperationException("Daily Review supports only tasks.");
        }

        if (item.Collection != ItemCollection.Today)
        {
            throw new InvalidOperationException("Only Today tasks can be kept in Daily Review.");
        }

        if (item.Status != ItemStatus.Open)
        {
            throw new InvalidOperationException("Only open Today tasks can be kept in Daily Review.");
        }

        await historyWriter.AppendHistoryAsync(
            item.Id,
            item.PublicRef.Value,
            "daily_reviewed",
            new
            {
                decision = "keep_today",
                collection = "today"
            },
            cancellationToken);

        return ItemResult.From(item);
    }
}
