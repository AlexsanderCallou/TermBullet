using TermBullet.Application.Configuration;
using TermBullet.Application.DataTransfer;
using TermBullet.Application.History;
using TermBullet.Application.Items;
using TermBullet.Application.Ports;
using TermBullet.Application.Startup;
using TermBullet.Cli;
using TermBullet.Infrastructure.Export;
using TermBullet.Infrastructure.Identity;
using TermBullet.Infrastructure.Persistence.JsonFiles;
using TermBullet.Tui;

namespace TermBullet.Bootstrap;

public static class TermBulletBootstrap
{
    public static TermBulletCliApp CreateCliApp(
        string projectRootPath,
        TextWriter output,
        TextWriter error)
    {
        var (clock, itemRepository, dataTransferService, historyMaintenanceService, settingsStore) =
            CreateSharedServices(projectRootPath);
        var startupMaintenanceUseCase = new RunStartupMaintenanceUseCase(clock, itemRepository);

        return new TermBulletCliApp(
            new ListConfigurationUseCase(settingsStore),
            new GetConfigurationUseCase(settingsStore),
            new SetConfigurationUseCase(settingsStore),
            new GetConfigurationPathUseCase(settingsStore),
            new ExportDataUseCase(dataTransferService),
            new ImportDataUseCase(dataTransferService),
            new ClearStoredHistoryUseCase(historyMaintenanceService, clock),
            output,
            error,
            new CreateItemUseCase(itemRepository, clock, new GuidIdGenerator()),
            new ListItemsUseCase(itemRepository),
            new ShowItemUseCase(itemRepository),
            new GetTodayItemsUseCase(itemRepository),
            new GetWeekItemsUseCase(itemRepository),
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
        var (clock, itemRepository, _, _, settingsStore) = CreateSharedServices(projectRootPath);
        var startupMaintenanceUseCase = new RunStartupMaintenanceUseCase(clock, itemRepository);

        return new TermBulletTuiApp(
            new GetTodayItemsUseCase(itemRepository),
            new GetBacklogItemsUseCase(itemRepository),
            new GetWeekItemsUseCase(itemRepository),
            new ListItemsUseCase(itemRepository),
            new SearchItemsUseCase(itemRepository),
            new ListConfigurationUseCase(settingsStore),
            new CreateItemUseCase(itemRepository, clock, new GuidIdGenerator()),
            new MarkDoneItemUseCase(itemRepository, clock),
            new CancelItemUseCase(itemRepository, clock),
            new MigrateItemUseCase(itemRepository, clock),
            new DeleteItemUseCase(itemRepository),
            startupAction: startupMaintenanceUseCase.ExecuteAsync);
    }

    private static (
        IClock Clock,
        JsonFileItemRepository ItemRepository,
        JsonDataTransferService DataTransferService,
        LocalHistoryMaintenanceService HistoryMaintenanceService,
        LocalSettingsStore SettingsStore)
        CreateSharedServices(string projectRootPath)
    {
        var fileStore = new SafeJsonFileStore();
        var clock = new SystemClock();
        var pathResolver = new MonthlyJsonFilePathResolver(projectRootPath);
        var indexService = new LocalJsonIndexService(projectRootPath, fileStore);
        var itemRepository = new JsonFileItemRepository(clock, pathResolver, fileStore, indexService);
        var dataTransferService = new JsonDataTransferService(
            projectRootPath, fileStore, new LocalJsonIndexService(projectRootPath, fileStore));
        var historyMaintenanceService = new LocalHistoryMaintenanceService(
            projectRootPath, pathResolver, fileStore);
        var settingsStore = new LocalSettingsStore(projectRootPath, fileStore);

        return (clock, itemRepository, dataTransferService, historyMaintenanceService, settingsStore);
    }

    private sealed class SystemClock : IClock
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
