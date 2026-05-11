using TermBullet.Application.Items;
using TermBullet.Application.Tags;
using TermBullet.Repositories.Interfaces;

namespace TermBullet.Tui;

public sealed class TuiSnapshotLoader(
    GetTodayItemsUseCase getTodayItemsUseCase,
    GetWeekItemsUseCase? getWeekItemsUseCase,
    GetMonthItemsUseCase? getMonthItemsUseCase,
    GetBacklogItemsUseCase getBacklogItemsUseCase,
    ListItemsUseCase? listItemsUseCase = null,
    ListTagsUseCase? listTagsUseCase = null,
    Func<CancellationToken, Task>? startupAction = null)
{
    private bool _startupCompleted;

    public async Task<TuiSnapshot> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!_startupCompleted && startupAction is not null)
        {
            await startupAction(cancellationToken);
            _startupCompleted = true;
        }

        var todayItems = await getTodayItemsUseCase.ExecuteAsync(cancellationToken);
        var weekItems = getWeekItemsUseCase is not null
            ? await getWeekItemsUseCase.ExecuteAsync(cancellationToken)
            : Array.Empty<ItemResult>();
        var monthItems = getMonthItemsUseCase is not null
            ? await getMonthItemsUseCase.ExecuteAsync(cancellationToken)
            : Array.Empty<ItemResult>();
        var backlogItems = await getBacklogItemsUseCase.ExecuteAsync(cancellationToken);
        var currentItems = listItemsUseCase is not null
            ? await listItemsUseCase.ExecuteAsync(new ListItemsRequest(), cancellationToken)
            : todayItems.Concat(weekItems).Concat(monthItems).Concat(backlogItems).ToArray();
        var allItems = listItemsUseCase is not null
            && listItemsUseCase.ItemRepository is IItemArchiveReader archiveReader
                ? (await archiveReader.ListAllAsync(cancellationToken)).Select(ItemResult.From).ToArray()
                : currentItems;
        var tags = listTagsUseCase is not null
            ? await listTagsUseCase.ExecuteAsync(cancellationToken)
            : Array.Empty<TagCatalogResult>();

        return new TuiSnapshot(todayItems, weekItems, monthItems, backlogItems, currentItems, allItems, tags);
    }
}
