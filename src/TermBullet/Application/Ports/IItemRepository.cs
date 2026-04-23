using TermBullet.Core.Items;

namespace TermBullet.Application.Ports;

public interface IItemRepository
{
    Task<int> GetCurrentPublicRefSequenceAsync(
        ItemType type,
        int month,
        int year,
        CancellationToken cancellationToken = default);

    Task<bool> PublicRefExistsAsync(
        string publicRef,
        CancellationToken cancellationToken = default);

    Task AddAsync(Item item, CancellationToken cancellationToken = default);

    Task UpdateAsync(Item item, CancellationToken cancellationToken = default);

    Task DeleteByPublicRefAsync(string publicRef, CancellationToken cancellationToken = default);

    Task ClearHistoryAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Item>> ListAsync(
        ItemCollection? collection = null,
        ItemStatus? status = null,
        CancellationToken cancellationToken = default);

    Task<Item?> FindByPublicRefAsync(
        string publicRef,
        CancellationToken cancellationToken = default);
}
