using Terminal.Gui;
using TermBullet.Application.Configuration;
using TermBullet.Application.Items;
using TermBullet.Tui.Navigation;
using TermBullet.Tui.Screens;
using TGui = Terminal.Gui.Application;

namespace TermBullet.Tui;

public sealed class TermBulletTuiApp(
    GetTodayItemsUseCase getTodayItemsUseCase,
    GetBacklogItemsUseCase getBacklogItemsUseCase,
    GetWeekItemsUseCase? getWeekItemsUseCase = null,
    ListItemsUseCase? listItemsUseCase = null,
    SearchItemsUseCase? searchItemsUseCase = null,
    ListConfigurationUseCase? listConfigurationUseCase = null,
    CreateItemUseCase? createItemUseCase = null,
    MarkDoneItemUseCase? markDoneItemUseCase = null,
    CancelItemUseCase? cancelItemUseCase = null,
    MigrateItemUseCase? migrateItemUseCase = null,
    DeleteItemUseCase? deleteItemUseCase = null,
    Func<CancellationToken, Task>? startupAction = null)
{
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var snapshotLoader = new TuiSnapshotLoader(
            getTodayItemsUseCase,
            getWeekItemsUseCase,
            getBacklogItemsUseCase,
            listItemsUseCase,
            listConfigurationUseCase,
            startupAction);
        var searchVm = new SearchViewModel();
        var navigation = new TuiNavigationState(panelCount: 6);

        MainDashboardActionHandler? actionHandler = null;
        if (markDoneItemUseCase is not null && cancelItemUseCase is not null
            && migrateItemUseCase is not null && deleteItemUseCase is not null)
        {
            actionHandler = new MainDashboardActionHandler(
                markDoneItemUseCase, cancelItemUseCase, migrateItemUseCase, deleteItemUseCase);
        }

        TGui.Init();
        try
        {
            await RunLoopAsync(snapshotLoader, searchVm, navigation, actionHandler, cancellationToken);
        }
        finally
        {
            TGui.Shutdown();
        }
    }

    private async Task RunLoopAsync(
        TuiSnapshotLoader snapshotLoader,
        SearchViewModel searchVm,
        TuiNavigationState navigation,
        MainDashboardActionHandler? actionHandler,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            var snapshot = await snapshotLoader.LoadAsync(cancellationToken);
            var dashboardVm = new MainDashboardViewModel(snapshot.TodayItems, snapshot.BacklogItems);
            var dailyFocusVm = new DailyFocusViewModel(snapshot.TodayItems);
            var weeklyPlanningVm = new WeeklyPlanningViewModel(snapshot.WeekItems, snapshot.BacklogItems);
            var backlogTriageVm = new BacklogTriageViewModel(snapshot.BacklogItems);
            var reviewVm = new ReviewViewModel(snapshot.AllItems);
            var configVm = new ConfigViewModel(snapshot.Configuration);
            var top = new Toplevel();

            switch (navigation.CurrentScreen)
            {
                case TuiScreen.DailyFocus:
                    DailyFocusScreen.Build(top, dailyFocusVm, navigation, () =>
                    {
                        navigation.NavigateBack();
                        TGui.RequestStop();
                    },
                    () =>
                    {
                        if (TryQuickCapture(navigation.CurrentScreen, createItemUseCase, cancellationToken))
                        {
                            TGui.RequestStop();
                        }
                    });
                    break;

                case TuiScreen.BacklogTriage:
                    BacklogTriageScreen.Build(top, backlogTriageVm, navigation, () =>
                    {
                        navigation.NavigateBack();
                        TGui.RequestStop();
                    },
                    () =>
                    {
                        if (TryQuickCapture(navigation.CurrentScreen, createItemUseCase, cancellationToken))
                        {
                            TGui.RequestStop();
                        }
                    });
                    break;

                case TuiScreen.WeeklyPlanning:
                    WeeklyPlanningScreen.Build(top, weeklyPlanningVm, navigation, () =>
                    {
                        navigation.NavigateBack();
                        TGui.RequestStop();
                    },
                    () =>
                    {
                        if (TryQuickCapture(navigation.CurrentScreen, createItemUseCase, cancellationToken))
                        {
                            TGui.RequestStop();
                        }
                    });
                    break;

                case TuiScreen.Review:
                    ReviewScreen.Build(top, reviewVm, navigation, () =>
                    {
                        navigation.NavigateBack();
                        TGui.RequestStop();
                    },
                    () =>
                    {
                        if (TryQuickCapture(navigation.CurrentScreen, createItemUseCase, cancellationToken))
                        {
                            TGui.RequestStop();
                        }
                    });
                    break;

                case TuiScreen.Search:
                    SearchScreen.Build(top, searchVm, navigation, () =>
                    {
                        navigation.NavigateBack();
                        TGui.RequestStop();
                    },
                    async query =>
                    {
                        if (searchItemsUseCase is not null && !string.IsNullOrWhiteSpace(query))
                        {
                            var results = await searchItemsUseCase.ExecuteAsync(
                                new SearchItemsRequest { Query = query }, cancellationToken);
                            searchVm.SetResults(results);
                        }
                    });
                    break;

                case TuiScreen.Config:
                    ConfigScreen.Build(top, configVm, navigation, () =>
                    {
                        navigation.NavigateBack();
                        TGui.RequestStop();
                    });
                    break;

                default:
                    BuildMainDashboard(top, dashboardVm, navigation, actionHandler, createItemUseCase, cancellationToken);
                    break;
            }

            TGui.Run(top);

            if (navigation.CurrentScreen == TuiScreen.MainDashboard && !navigation.CanNavigateBack)
                break;
        }
    }

    private static void BuildMainDashboard(
        Toplevel top,
        MainDashboardViewModel viewModel,
        TuiNavigationState navigation,
        MainDashboardActionHandler? actionHandler,
        CreateItemUseCase? createItemUseCase,
        CancellationToken cancellationToken)
    {
        var date = DateTime.Today.ToString("yyyy-MM-dd");
        var topBar = new Label($" TermBullet \u2500 Daily {date} \u2500 data:local \u2500 ai:off \u2500 sync:idle \u2500 mode:normal")
        {
            X = 0, Y = 0, Width = Dim.Fill()
        };

        var footer = new Label(" / filter  c capture  e edit  x done  > migrate  Enter zoom  Tab focus  ? help  q quit")
        {
            X = 0, Y = Pos.AnchorEnd(1), Width = Dim.Fill()
        };

        var upperHeight = Dim.Percent(55);

        // Panel 1: Collections
        var collectionsPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(1, "Collections", navigation, 0))
        {
            X = 0, Y = 1, Width = Dim.Percent(20), Height = upperHeight
        };
        var collectionEntries = new[] { "> Daily", "  Weekly", "  Monthly", "  Backlog", "  Review", "  Search", "  Config" };
        var collectionsList = new ListView(collectionEntries)
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };
        collectionsPanel.Add(collectionsList);

        // Panel 2: Day Items
        var dayItemsPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(2, "Day Items", navigation, 1))
        {
            X = Pos.Right(collectionsPanel), Y = 1, Width = Dim.Percent(50), Height = upperHeight
        };
        var dayItemRows = viewModel.DayItems.Count > 0
            ? viewModel.DayItems.Select(r => $"{r.Symbol} {r.PublicRef} {r.Content}").ToArray()
            : new[] { "(no items)" };
        var dayItemsList = new ListView(dayItemRows)
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };
        if (viewModel.SelectedDayItemIndex >= 0)
            dayItemsList.SelectedItem = viewModel.SelectedDayItemIndex;

        dayItemsPanel.Add(dayItemsList);

        // Panel 3: Preview
        var previewPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(3, "Preview / AI", navigation, 2))
        {
            X = Pos.Right(dayItemsPanel), Y = 1, Width = Dim.Fill(), Height = upperHeight
        };
        var previewList = new ListView(BuildPreviewLines(viewModel.SelectedDayItem))
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };
        previewPanel.Add(previewList);

        var projectsPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(4, "Projects / Tags", navigation, 3))
        {
            X = 0, Y = Pos.Bottom(collectionsPanel), Width = Dim.Percent(20), Height = Dim.Fill(1)
        };
        var projectsList = new ListView(
            viewModel.ProjectOrTagRows.Count > 0
                ? viewModel.ProjectOrTagRows.Select(tag => $"> {tag}").ToArray()
                : ["(no tags yet)"])
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };
        projectsPanel.Add(projectsList);

        var filteredBacklogPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(5, "Filtered Backlog", navigation, 4))
        {
            X = Pos.Right(projectsPanel), Y = Pos.Bottom(dayItemsPanel), Width = Dim.Percent(50), Height = Dim.Fill(1)
        };
        var filteredBacklogList = new ListView(
            viewModel.FilteredBacklogItems.Count > 0
                ? viewModel.FilteredBacklogItems.Select(r => $"{r.Symbol} {r.PublicRef} {r.Content}").ToArray()
                : ["(no related backlog)"])
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };
        filteredBacklogPanel.Add(filteredBacklogList);

        var suggestedPlanPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(6, "Suggested Plan", navigation, 5))
        {
            X = Pos.Right(filteredBacklogPanel), Y = Pos.Bottom(previewPanel), Width = Dim.Fill(), Height = Dim.Fill(1)
        };
        var suggestedPlanList = new ListView(viewModel.SuggestedPlanLines.ToArray())
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };
        suggestedPlanPanel.Add(suggestedPlanList);

        var panels = new[]
        {
            collectionsPanel, dayItemsPanel, previewPanel, projectsPanel, filteredBacklogPanel, suggestedPlanPanel
        };
        var panelTitles = new[]
        {
            "Collections", "Day Items", "Preview / AI", "Projects / Tags", "Filtered Backlog", "Suggested Plan"
        };
        var focusTargets = new View[]
        {
            collectionsList, dayItemsList, previewList, projectsList, filteredBacklogList, suggestedPlanList
        };
        TuiScreenUtilities.UpdatePanelTitles(panels, panelTitles, navigation);
        TuiScreenUtilities.FocusCurrentPanel(focusTargets, navigation);

        top.Add(topBar, collectionsPanel, dayItemsPanel, previewPanel, projectsPanel, filteredBacklogPanel, suggestedPlanPanel, footer);

        dayItemsList.SelectedItemChanged += _ =>
        {
            var newIndex = dayItemsList.SelectedItem;
            if (newIndex < 0 || newIndex >= viewModel.DayItems.Count) return;
            var diff = newIndex - viewModel.SelectedDayItemIndex;
            if (diff > 0)
                for (var i = 0; i < diff; i++) viewModel.SelectNextDayItem();
            else if (diff < 0)
                for (var i = 0; i < -diff; i++) viewModel.SelectPreviousDayItem();

            TuiScreenUtilities.RefreshListView(previewList, BuildPreviewLines(viewModel.SelectedDayItem));
            TuiScreenUtilities.RefreshListView(
                filteredBacklogList,
                viewModel.FilteredBacklogItems.Count > 0
                    ? viewModel.FilteredBacklogItems.Select(r => $"{r.Symbol} {r.PublicRef} {r.Content}").ToArray()
                    : ["(no related backlog)"]);
            TuiScreenUtilities.RefreshListView(suggestedPlanList, viewModel.SuggestedPlanLines.ToArray());
        };

        top.KeyPress += args =>
        {
            switch (args.KeyEvent.Key)
            {
                case Key.q:
                    TGui.RequestStop();
                    args.Handled = true;
                    break;

                case Key.Tab:
                    navigation.MoveNextPanel();
                    TuiScreenUtilities.UpdatePanelTitles(panels, panelTitles, navigation);
                    TuiScreenUtilities.FocusCurrentPanel(focusTargets, navigation);
                    args.Handled = true;
                    break;

                case Key.BackTab:
                    navigation.MovePreviousPanel();
                    TuiScreenUtilities.UpdatePanelTitles(panels, panelTitles, navigation);
                    TuiScreenUtilities.FocusCurrentPanel(focusTargets, navigation);
                    args.Handled = true;
                    break;

                case Key.Enter:
                    NavigateFromCollections(collectionsList.SelectedItem, navigation);
                    args.Handled = true;
                    break;

                case Key x when x == (Key)'/' :
                    navigation.NavigateTo(TuiScreen.Search, panelCount: 2);
                    TGui.RequestStop();
                    args.Handled = true;
                    break;

                case Key x when x == (Key)'?':
                    TuiScreenUtilities.ShowContextHelp(TuiScreen.MainDashboard);
                    args.Handled = true;
                    break;

                case Key x when x == (Key)'c' && createItemUseCase is not null:
                    if (TryQuickCapture(TuiScreen.MainDashboard, createItemUseCase, cancellationToken))
                    {
                        TGui.RequestStop();
                    }
                    args.Handled = true;
                    break;

                case Key x when x == (Key)'x' && actionHandler is not null:
                    DispatchAction(viewModel.SelectedDayItem?.PublicRef, actionHandler.HandleDoneAsync, cancellationToken);
                    args.Handled = true;
                    break;

                case Key y when y == (Key)'>' && actionHandler is not null:
                    DispatchAction(viewModel.SelectedDayItem?.PublicRef, actionHandler.HandleMigrateAsync, cancellationToken);
                    args.Handled = true;
                    break;

                case Key z when z == (Key)'d' && actionHandler is not null:
                    DispatchAction(viewModel.SelectedDayItem?.PublicRef, actionHandler.HandleDeleteAsync, cancellationToken);
                    args.Handled = true;
                    break;
            }
        };
    }

    private static void NavigateFromCollections(int selectedIndex, TuiNavigationState navigation)
    {
        var screen = selectedIndex switch
        {
            0 => TuiScreen.DailyFocus,
            1 => TuiScreen.WeeklyPlanning,
            3 => TuiScreen.BacklogTriage,
            4 => TuiScreen.Review,
            5 => TuiScreen.Search,
            6 => TuiScreen.Config,
            _ => (TuiScreen?)null
        };

        if (screen.HasValue)
        {
            var panelCount = screen.Value switch
            {
                TuiScreen.DailyFocus => 6,
                TuiScreen.WeeklyPlanning => 6,
                TuiScreen.Review => 6,
                TuiScreen.Search => 2,
                TuiScreen.Config => 3,
                _ => 3
            };
            navigation.NavigateTo(screen.Value, panelCount);
            TGui.RequestStop();
        }
    }

    private static void DispatchAction(
        string? publicRef,
        Func<string, CancellationToken, Task<ActionResult>> handler,
        CancellationToken cancellationToken)
    {
        if (publicRef is null) return;
        _ = Task.Run(async () =>
        {
            await handler(publicRef, cancellationToken);
            TGui.MainLoop?.Invoke(() => TGui.RequestStop());
        }, cancellationToken);
    }

    private static string[] BuildPreviewLines(ItemDisplayRow? item) =>
        item is not null
            ? new[]
            {
                item.PublicRef,
                $"type: {item.Type}",
                $"status: {item.Status}",
                $"priority: {item.Priority}",
                $"collection: {item.Collection}",
                $"tags: {(item.Tags.Length > 0 ? string.Join(", ", item.Tags) : "(none)")}"
            }
            : new[] { "(nothing selected)" };

    private static bool TryQuickCapture(
        TuiScreen screen,
        CreateItemUseCase? createItemUseCase,
        CancellationToken cancellationToken)
    {
        if (createItemUseCase is null)
        {
            return false;
        }

        return QuickCaptureDialog.Show(screen, createItemUseCase, cancellationToken);
    }

}
