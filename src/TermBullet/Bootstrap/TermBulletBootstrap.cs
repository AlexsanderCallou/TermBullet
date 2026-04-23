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

namespace TermBullet.Bootstrap;

public static class TermBulletBootstrap
{
    public static TermBulletCliApp CreateCliApp(
        string projectRootPath,
        TextWriter output,
        TextWriter error)
    {
        var fileStore = new SafeJsonFileStore();
        var settingsStore = new LocalSettingsStore(projectRootPath, fileStore);
        var clock = new SystemClock();
        var itemRepository = new JsonFileItemRepository(
            clock,
            new MonthlyJsonFilePathResolver(projectRootPath),
            fileStore,
            new LocalJsonIndexService(projectRootPath, fileStore));
        var dataTransferService = new JsonDataTransferService(
            projectRootPath,
            fileStore,
            new LocalJsonIndexService(projectRootPath, fileStore));
        var historyMaintenanceService = new LocalHistoryMaintenanceService(
            projectRootPath,
            new MonthlyJsonFilePathResolver(projectRootPath),
            fileStore);
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

    private sealed class SystemClock : IClock
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
