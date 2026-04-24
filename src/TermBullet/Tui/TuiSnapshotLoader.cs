using TermBullet.Application.Configuration;
using TermBullet.Application.Items;

namespace TermBullet.Tui;

public sealed class TuiSnapshotLoader(
    GetTodayItemsUseCase getTodayItemsUseCase,
    GetWeekItemsUseCase? getWeekItemsUseCase,
    GetBacklogItemsUseCase getBacklogItemsUseCase,
    ListItemsUseCase? listItemsUseCase = null,
    ListConfigurationUseCase? listConfigurationUseCase = null,
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
        var backlogItems = await getBacklogItemsUseCase.ExecuteAsync(cancellationToken);
        var allItems = listItemsUseCase is not null
            ? await listItemsUseCase.ExecuteAsync(new ListItemsRequest(), cancellationToken)
            : todayItems.Concat(weekItems).Concat(backlogItems).ToArray();
        IReadOnlyDictionary<string, string> configuration = new Dictionary<string, string>();

        if (listConfigurationUseCase is not null)
        {
            configuration = await listConfigurationUseCase.ExecuteAsync("default", cancellationToken);
        }

        return new TuiSnapshot(todayItems, weekItems, backlogItems, allItems, configuration);
    }
}
