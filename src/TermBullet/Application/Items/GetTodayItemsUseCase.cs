using TermBullet.Repositories.Interfaces;
using TermBullet.Domain.Items;
using TermBullet.Services.Clock;

namespace TermBullet.Application.Items;

public sealed class GetTodayItemsUseCase(IItemRepository itemRepository, IClock clock)
{
    public async Task<IReadOnlyCollection<ItemResult>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var items = await itemRepository.ListAsync(
            collection: ItemCollection.Today,
            cancellationToken: cancellationToken);

        var currentLocalDate = ToLocalDate(clock.UtcNow);

        return items
            .Where(item => ShouldShowInToday(item, currentLocalDate))
            .Select(ItemResult.From)
            .ToArray();
    }

    private static bool ShouldShowInToday(Item item, DateOnly currentLocalDate)
    {
        return item.Status switch
        {
            ItemStatus.Open => true,
            ItemStatus.Done => IsSameLocalDate(item.CompletedAt ?? item.UpdatedAt, currentLocalDate),
            ItemStatus.Cancelled => IsSameLocalDate(item.CancelledAt ?? item.UpdatedAt, currentLocalDate),
            _ => false
        };
    }

    private static bool IsSameLocalDate(DateTimeOffset value, DateOnly currentLocalDate) =>
        ToLocalDate(value) == currentLocalDate;

    private static DateOnly ToLocalDate(DateTimeOffset value) =>
        DateOnly.FromDateTime(value.ToLocalTime().DateTime);
}
