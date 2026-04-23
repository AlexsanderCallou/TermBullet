using TermBullet.Application.Ports;

namespace TermBullet.Application.Items;

public sealed class ClearHistoryUseCase(IItemRepository itemRepository)
{
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return itemRepository.ClearHistoryAsync(cancellationToken);
    }
}
