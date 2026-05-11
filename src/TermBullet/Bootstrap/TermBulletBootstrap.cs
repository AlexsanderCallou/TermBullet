using TermBullet.Services.Clock;
using TermBullet.Application.History;
using TermBullet.Application.Items;
using TermBullet.Repositories.Interfaces;
using TermBullet.Application.Startup;
using TermBullet.Application.Tags;
using TermBullet.Cli;
using TermBullet.Services.Ids;
using TermBullet.Repositories.Json;
using TermBullet.Tui;

namespace TermBullet.Bootstrap;

public static class TermBulletBootstrap
{
    public static TermBulletCliApp CreateCliApp(
        string projectRootPath,
        TextWriter output,
        TextWriter error)
    {
        var (clock, itemRepository, tagCatalogRepository, historyMaintenanceService) =
            CreateSharedServices(projectRootPath);
        var startupMaintenanceUseCase = new RunStartupMaintenanceUseCase(clock, itemRepository);

        return new TermBulletCliApp(
            new ClearStoredHistoryUseCase(historyMaintenanceService, clock),
            output,
            error,
            new CreateItemUseCase(itemRepository, clock, new GuidIdGenerator()),
            new ListItemsUseCase(itemRepository),
            new ShowItemUseCase(itemRepository),
            new GetTodayItemsUseCase(itemRepository),
            new GetWeekItemsUseCase(itemRepository),
            new GetMonthItemsUseCase(itemRepository),
            new GetBacklogItemsUseCase(itemRepository),
            new EditItemUseCase(itemRepository, clock),
            new MarkDoneItemUseCase(itemRepository, clock),
            new CancelItemUseCase(itemRepository, clock),
            new MoveItemUseCase(itemRepository, clock),
            new SetItemPriorityUseCase(itemRepository, clock),
            new TagItemUseCase(itemRepository, clock),
            new UntagItemUseCase(itemRepository, clock),
            new MigrateItemUseCase(itemRepository, clock),
            new DeleteItemUseCase(itemRepository),
            new SearchItemsUseCase(itemRepository),
            startupAction: startupMaintenanceUseCase.ExecuteAsync);
    }

    public static TermBulletTuiApp CreateTuiApp(string projectRootPath)
    {
        var (clock, itemRepository, tagCatalogRepository, _) = CreateSharedServices(projectRootPath);
        var startupMaintenanceUseCase = new RunStartupMaintenanceUseCase(clock, itemRepository);

        return new TermBulletTuiApp(
            new GetTodayItemsUseCase(itemRepository),
            new GetBacklogItemsUseCase(itemRepository),
            new GetWeekItemsUseCase(itemRepository),
            new GetMonthItemsUseCase(itemRepository),
            new ListItemsUseCase(itemRepository),
            new SearchItemsUseCase(itemRepository),
            new ListTagsUseCase(tagCatalogRepository),
            new CreateTagUseCase(tagCatalogRepository, clock),
            new CreateItemUseCase(itemRepository, clock, new GuidIdGenerator()),
            new EditItemUseCase(itemRepository, clock),
            new MarkDoneItemUseCase(itemRepository, clock),
            new CancelItemUseCase(itemRepository, clock),
            new MigrateItemUseCase(itemRepository, clock),
            new DeleteItemUseCase(itemRepository),
            startupAction: startupMaintenanceUseCase.ExecuteAsync);
    }

    private static (
        IClock Clock,
        JsonItemRepository ItemRepository,
        JsonTagCatalogRepository TagCatalogRepository,
        JsonHistoryMaintenanceService HistoryMaintenanceService)
        CreateSharedServices(string projectRootPath)
    {
        var fileStore = new JsonFileStore();
        var clock = new SystemClock();
        var pathResolver = new MonthlyJsonPathResolver(projectRootPath);
        var indexService = new JsonIndexService(projectRootPath, fileStore);
        var itemRepository = new JsonItemRepository(clock, pathResolver, fileStore, indexService);
        var tagCatalogRepository = new JsonTagCatalogRepository(projectRootPath, fileStore);
        var historyMaintenanceService = new JsonHistoryMaintenanceService(
            projectRootPath, pathResolver, fileStore);

        return (clock, itemRepository, tagCatalogRepository, historyMaintenanceService);
    }

    private sealed class SystemClock : IClock
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
