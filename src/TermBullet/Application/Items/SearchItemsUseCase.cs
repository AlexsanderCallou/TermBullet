using TermBullet.Application.Ports;
using TermBullet.Core.Items;

namespace TermBullet.Application.Items;

public sealed class SearchItemsUseCase(IItemRepository itemRepository)
{
    public async Task<IReadOnlyCollection<ItemResult>> ExecuteAsync(
        SearchItemsRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var normalizedQuery = NormalizeQuery(request.Query);
        var items = await itemRepository.ListAsync(cancellationToken: cancellationToken);

        return items
            .Where(item => Matches(item, normalizedQuery))
            .Select(ItemResult.From)
            .ToArray();
    }

    private static string NormalizeQuery(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("Query must not be empty.", nameof(query));
        }

        return query.Trim();
    }

    private static bool Matches(Item item, string query)
    {
        var comparison = StringComparison.OrdinalIgnoreCase;

        return item.PublicRef.Value.Contains(query, comparison)
            || item.Content.Contains(query, comparison)
            || (item.Description?.Contains(query, comparison) ?? false)
            || item.Tags.Any(tag => tag.Contains(query, comparison));
    }
}
