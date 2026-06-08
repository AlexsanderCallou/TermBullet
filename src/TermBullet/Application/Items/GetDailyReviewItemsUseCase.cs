using System.Text.Json;
using TermBullet.Domain.Items;
using TermBullet.Repositories.Interfaces;
using TermBullet.Services.Clock;

namespace TermBullet.Application.Items;

public sealed class GetDailyReviewItemsUseCase(
    IItemRepository itemRepository,
    IItemHistoryReader historyReader,
    IClock clock)
{
    public async Task<IReadOnlyCollection<DailyReviewItemResult>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var currentLocalDate = ToLocalDate(clock.UtcNow);
        var items = await itemRepository.ListAsync(
            collection: ItemCollection.Today,
            status: ItemStatus.Open,
            cancellationToken: cancellationToken);

        var result = new List<DailyReviewItemResult>();
        foreach (var item in items)
        {
            var history = await historyReader.ListHistoryByPublicRefAsync(item.PublicRef.Value, cancellationToken);
            var latestTodayPlacementDate = GetLatestTodayPlacementDate(item, history);
            if (latestTodayPlacementDate < currentLocalDate)
            {
                result.Add(new DailyReviewItemResult(ItemResult.From(item), latestTodayPlacementDate));
            }
        }

        return result
            .OrderBy(item => item.LastTodayPlacementDate)
            .ThenBy(item => item.Item.PublicRef, StringComparer.Ordinal)
            .ToArray();
    }

    private static DateOnly GetLatestTodayPlacementDate(Item item, IReadOnlyCollection<ItemHistoryEntry> history)
    {
        var latestTodayPlacement = history
            .Where(IsTodayPlacementOrReview)
            .Select(entry => entry.OccurredAt)
            .DefaultIfEmpty(item.CreatedAt)
            .Max();

        return ToLocalDate(latestTodayPlacement);
    }

    private static bool IsTodayPlacementOrReview(ItemHistoryEntry entry)
    {
        return entry.EventType switch
        {
            "created" => HasDataValue(entry.DataJson, "collection", "today"),
            "carried_over" => HasDataValue(entry.DataJson, "collection", "today"),
            "migrate" => HasDataValue(entry.DataJson, "to_collection", "today"),
            "daily_reviewed" => HasDataValue(entry.DataJson, "collection", "today"),
            _ => false
        };
    }

    private static bool HasDataValue(string json, string propertyName, string expectedValue)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.TryGetProperty(propertyName, out var property)
                && property.ValueKind == JsonValueKind.String
                && string.Equals(property.GetString(), expectedValue, StringComparison.OrdinalIgnoreCase);
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static DateOnly ToLocalDate(DateTimeOffset value) =>
        DateOnly.FromDateTime(value.ToLocalTime().DateTime);
}
