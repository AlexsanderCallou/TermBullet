using TermBullet.Application.Ports;
using TermBullet.Core.Items;

namespace TermBullet.Application.Items;

public sealed class ListItemsUseCase(IItemRepository itemRepository)
{
    public async Task<IReadOnlyCollection<ItemResult>> ExecuteAsync(
        ListItemsRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidateFilter(request.Collection, nameof(request.Collection));
        ValidateFilter(request.Status, nameof(request.Status));

        var items = await itemRepository.ListAsync(
            request.Collection,
            request.Status,
            cancellationToken);

        return items.Select(ItemResult.From).ToArray();
    }

    private static void ValidateFilter<TEnum>(TEnum? value, string propertyName)
        where TEnum : struct, Enum
    {
        if (value is null || Enum.IsDefined(value.Value))
        {
            return;
        }

        var parameterName = char.ToLowerInvariant(propertyName[0]) + propertyName[1..];
        throw new ArgumentOutOfRangeException(
            parameterName,
            value,
            $"Unsupported {typeof(TEnum).Name} filter.");
    }
}
