using TermBullet.Services.Clock;
using TermBullet.Application.Ai;
using TermBullet.Application.History;
using TermBullet.Application.Items;
using TermBullet.Repositories.Interfaces;
using TermBullet.Application.Startup;
using TermBullet.Application.Tags;
using TermBullet.Cli;
using TermBullet.Services.Ids;
using TermBullet.Repositories.Json;
using TermBullet.Services.Ai;
using TermBullet.Services.Configuration;
using TermBullet.Tui;

namespace TermBullet.Bootstrap;

public static class TermBulletBootstrap
{
    public static bool IsInformationalCliRequest(string[] args)
    {
        return args.Any(arg =>
            string.Equals(arg, "-h", StringComparison.Ordinal)
            || string.Equals(arg, "--help", StringComparison.Ordinal)
            || string.Equals(arg, "-v", StringComparison.Ordinal)
            || string.Equals(arg, "--version", StringComparison.Ordinal));
    }

    public static TermBulletCliApp CreateInformationalCliApp(TextWriter output, TextWriter error)
    {
        var runtimePaths = new TermBulletRuntimePaths(
            Path.Combine(AppContext.BaseDirectory, "conf.json"),
            AppContext.BaseDirectory,
            Path.Combine(AppContext.BaseDirectory, "data"));
        return CreateCliApp(runtimePaths, output, error, startupAction: _ => Task.CompletedTask);
    }

    public static TermBulletCliApp CreateCliApp(
        TermBulletRuntimePaths runtimePaths,
        TextWriter output,
        TextWriter error,
        Func<CancellationToken, Task>? startupAction = null)
    {
        var (clock, itemRepository, tagCatalogRepository, historyMaintenanceService) =
            CreateSharedServices(runtimePaths.DataRoot);
        var startupMaintenanceUseCase = new RunStartupMaintenanceUseCase(clock, itemRepository);
        var installDirectory = Path.GetDirectoryName(runtimePaths.ConfigPath)
            ?? throw new InvalidOperationException("Runtime config path is invalid.");
        var aiConfigurationFileService = new AiConfigurationFileService(runtimePaths.DataRoot);

        return new TermBulletCliApp(
            new ClearStoredHistoryUseCase(historyMaintenanceService, clock),
            output,
            error,
            new CreateItemUseCase(itemRepository, clock, new GuidIdGenerator()),
            new ListItemsUseCase(itemRepository),
            new ShowItemUseCase(itemRepository),
            new GetTodayItemsUseCase(itemRepository, clock),
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
            runtimePaths,
            generateAiPlanningDraft: (request, cancellationToken) => GenerateAiPlanningDraftAsync(
                request,
                itemRepository,
                installDirectory,
                cancellationToken),
            generateAiPlanningResponse: (request, cancellationToken) => GenerateAiPlanningResponseAsync(
                request,
                itemRepository,
                installDirectory,
                cancellationToken),
            applyAiPlanningDraft: (draft, cancellationToken) => ApplyAiPlanningDraftAsync(
                draft,
                itemRepository,
                tagCatalogRepository,
                clock,
                cancellationToken),
            testAiProfileConnection: (profileName, cancellationToken) => TestAiProfileConnectionAsync(
                aiConfigurationFileService,
                profileName,
                cancellationToken),
            startupAction: startupAction ?? startupMaintenanceUseCase.ExecuteAsync,
            getDailyReviewItemsUseCase: new GetDailyReviewItemsUseCase(itemRepository, itemRepository, clock),
            keepTodayItemUseCase: new KeepTodayItemUseCase(itemRepository, itemRepository));
    }

    public static TermBulletTuiApp CreateTuiApp(TermBulletRuntimePaths runtimePaths)
    {
        var (clock, itemRepository, tagCatalogRepository, _) = CreateSharedServices(runtimePaths.DataRoot);
        var startupMaintenanceUseCase = new RunStartupMaintenanceUseCase(clock, itemRepository);
        var installDirectory = Path.GetDirectoryName(runtimePaths.ConfigPath)
            ?? throw new InvalidOperationException("Runtime config path is invalid.");

        return new TermBulletTuiApp(
            new GetTodayItemsUseCase(itemRepository, clock),
            new GetBacklogItemsUseCase(itemRepository),
            new GetWeekItemsUseCase(itemRepository),
            new GetMonthItemsUseCase(itemRepository),
            new GetDailyReviewItemsUseCase(itemRepository, itemRepository, clock),
            new KeepTodayItemUseCase(itemRepository, itemRepository),
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
            new ShowItemHistoryUseCase(itemRepository),
            generateAiPlanningResponse: (request, cancellationToken) => GenerateAiPlanningResponseAsync(
                request,
                itemRepository,
                installDirectory,
                cancellationToken),
            applyAiPlanningDraft: (draft, cancellationToken) => ApplyAiPlanningDraftAsync(
                draft,
                itemRepository,
                tagCatalogRepository,
                clock,
                cancellationToken),
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

    private static async Task<GenerateAiPlanningDraftResult> GenerateAiPlanningDraftAsync(
        BuildAiPlanningRequest request,
        IItemRepository itemRepository,
        string installDirectory,
        CancellationToken cancellationToken = default)
    {
        var result = await GenerateAiPlanningResponseAsync(
            WithStructuredDraftRequired(request),
            itemRepository,
            installDirectory,
            cancellationToken);
        if (result.Draft is null)
        {
            throw new InvalidOperationException("AI planning response did not include a structured draft.");
        }

        return new GenerateAiPlanningDraftResult(result.Draft, result.ProviderModel, result.ModelRequest);
    }

    private static async Task<GenerateAiPlanningResponseResult> GenerateAiPlanningResponseAsync(
        BuildAiPlanningRequest request,
        IItemRepository itemRepository,
        string installDirectory,
        CancellationToken cancellationToken = default)
    {
        var config = await LoadAiConfigAsync(installDirectory, cancellationToken);
        var providerFactory = new AiPlanningProviderFactory(() => CreateAiHttpClient(config));
        var provider = providerFactory.Create(config);
        var useCase = new GenerateAiPlanningResponseUseCase(
            new BuildAiPlanningRequestUseCase(
                new PlanningAgentPromptLoader(installDirectory)),
            provider,
            new AiPlanningDraftValidator());

        return await useCase.ExecuteAsync(request, cancellationToken);
    }

    private static BuildAiPlanningRequest WithStructuredDraftRequired(BuildAiPlanningRequest request) =>
        new()
        {
            Mode = request.Mode,
            Tag = request.Tag,
            UserPrompt = request.UserPrompt,
            ConversationHistory = request.ConversationHistory,
            RequireStructuredDraft = true
        };

    private static async Task<AiPlanningDraftApplyResult> ApplyAiPlanningDraftAsync(
        AiPlanningDraft draft,
        IItemRepository itemRepository,
        ITagCatalogRepository tagCatalogRepository,
        IClock clock,
        CancellationToken cancellationToken = default)
    {
        var useCase = new ApplyAiPlanningDraftUseCase(
            new AiPlanningDraftValidator(),
            new CreateTagUseCase(tagCatalogRepository, clock),
            new CreateItemUseCase(itemRepository, clock, new GuidIdGenerator()));

        return await useCase.ExecuteAsync(draft, cancellationToken);
    }

    private static async Task<TermBulletConfig> LoadAiConfigAsync(
        string installDirectory,
        CancellationToken cancellationToken)
    {
        var legacyConfigService = new TermBulletConfigService(installDirectory);
        var legacyConfig = await legacyConfigService.LoadAsync(cancellationToken);
        var dataRoot = legacyConfig?.DataRoot ?? Path.Combine(installDirectory, "data");
        return await new AiConfigurationFileService(dataRoot).LoadConfigAsync(cancellationToken);
    }

    private static HttpClient CreateAiHttpClient(TermBulletConfig config)
    {
        var timeoutSeconds = 270;
        if (config.Ai is not null
            && !string.IsNullOrWhiteSpace(config.Ai.ActiveProfile)
            && config.Ai.Profiles.TryGetValue(config.Ai.ActiveProfile, out var profile)
            && profile.TimeoutSeconds is > 0)
        {
            timeoutSeconds = profile.TimeoutSeconds.Value;
        }

        return new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(timeoutSeconds)
        };
    }

    private static async Task<AiPlanningProviderResponse> TestAiProfileConnectionAsync(
        AiConfigurationFileService aiConfigurationFileService,
        string? profileName,
        CancellationToken cancellationToken = default)
    {
        var config = await aiConfigurationFileService.LoadConfigOrCreateTemplateAsync(cancellationToken);
        var profiles = config.Ai?.Profiles ?? new Dictionary<string, AiProfile>();
        var activeProfile = string.IsNullOrWhiteSpace(profileName)
            ? config.Ai?.ActiveProfile
            : profileName;

        if (string.IsNullOrWhiteSpace(activeProfile))
        {
            throw new InvalidOperationException("No active AI profile is configured.");
        }

        if (!profiles.ContainsKey(activeProfile))
        {
            throw new InvalidOperationException($"AI profile not found: {activeProfile}");
        }

        var testProfile = profiles[activeProfile];
        var testConfig = config with
        {
            Ai = new AiConfiguration(activeProfile, profiles)
        };
        var provider = new AiPlanningProviderFactory(() => CreateAiHttpClient(testConfig)).Create(testConfig);

        return await provider.SendAsync(new AiPlanningModelRequest(
            AiPlanningMode.NewWeekly,
            Tag: null,
            Messages:
            [
                new(AiPlanningMessageRole.Agent, "You are validating TermBullet AI connectivity. Reply with OK."),
                new(AiPlanningMessageRole.User, "Reply with OK.")
            ],
            ContextItems: [],
            RequireStructuredDraft: false,
            MaxOutputTokens: testProfile.TestMaxTokens ?? (testProfile.Reasoning ? 128 : 64)), cancellationToken);
    }

    private sealed class SystemClock : IClock
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
