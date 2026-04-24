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
        var snapshot = await snapshotLoader.LoadAsync(cancellationToken);
        var searchVm = new SearchViewModel();
        var navigation = new TuiNavigationState(panelCount: 6);
        var addSourceScreen = TuiScreen.MainDashboard;
        string? addError = null;

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
            var top = new Toplevel();
            var host = new TuiScreenHost(top);

            void ScheduleRender()
            {
                _ = Task.Run(() =>
                {
                    TGui.MainLoop?.Invoke(Render);
                }, cancellationToken);
            }

            void NavigateTo(TuiScreen screen, int panelCount)
            {
                navigation.NavigateTo(screen, panelCount);
                ScheduleRender();
            }

            void NavigateBack()
            {
                navigation.NavigateBack();
                ScheduleRender();
            }

            void OpenAddItem(TuiScreen sourceScreen)
            {
                addSourceScreen = sourceScreen;
                addError = null;
                navigation.NavigateTo(TuiScreen.AddItem, panelCount: 1);
                ScheduleRender();
            }

            void Quit() => TGui.RequestStop();

            void RefreshAndRender()
            {
                _ = Task.Run(async () =>
                {
                    var refreshed = await snapshotLoader.LoadAsync(cancellationToken);
                    TGui.MainLoop?.Invoke(() =>
                    {
                        snapshot = refreshed;
                        ScheduleRender();
                    });
                }, cancellationToken);
            }

            TGui.RootKeyEvent = keyEvent =>
            {
                if (navigation.CurrentScreen == TuiScreen.AddItem)
                {
                    if (keyEvent.Key == Key.Esc)
                    {
                        addError = null;
                        NavigateBack();
                        return true;
                    }

                    return false;
                }

                if (TuiScreenUtilities.IsHelpKey(keyEvent))
                {
                    TuiScreenUtilities.ShowContextHelp(navigation.CurrentScreen);
                    return true;
                }

                if (keyEvent.Key == Key.q)
                {
                    Quit();
                    return true;
                }

                if (keyEvent.Key == Key.Esc && navigation.CurrentScreen != TuiScreen.MainDashboard)
                {
                    NavigateBack();
                    return true;
                }

                if (keyEvent.Key == (Key)'c' && createItemUseCase is not null)
                {
                    OpenAddItem(navigation.CurrentScreen);
                    return true;
                }

                if (keyEvent.Key == (Key)'/' && navigation.CurrentScreen == TuiScreen.MainDashboard)
                {
                    NavigateTo(TuiScreen.Search, GetPanelCount(TuiScreen.Search));
                    return true;
                }

                return false;
            };

            void Render()
            {
                var root = host.ReplaceContent();
                var dashboardVm = new MainDashboardViewModel(snapshot.TodayItems, snapshot.BacklogItems);

                switch (navigation.CurrentScreen)
                {
                    case TuiScreen.DailyFocus:
                        DailyFocusScreen.Build(
                            root,
                            new DailyFocusViewModel(snapshot.TodayItems),
                            navigation,
                            NavigateBack,
                            Quit,
                            () => OpenAddItem(TuiScreen.DailyFocus));
                        break;

                    case TuiScreen.WeeklyPlanning:
                        WeeklyPlanningScreen.Build(
                            root,
                            new WeeklyPlanningViewModel(snapshot.WeekItems, snapshot.BacklogItems),
                            navigation,
                            NavigateBack,
                            Quit,
                            () => OpenAddItem(TuiScreen.WeeklyPlanning));
                        break;

                    case TuiScreen.BacklogTriage:
                        BacklogTriageScreen.Build(
                            root,
                            new BacklogTriageViewModel(snapshot.BacklogItems),
                            navigation,
                            NavigateBack,
                            Quit,
                            () => OpenAddItem(TuiScreen.BacklogTriage));
                        break;

                    case TuiScreen.Review:
                        ReviewScreen.Build(
                            root,
                            new ReviewViewModel(snapshot.AllItems),
                            navigation,
                            NavigateBack,
                            Quit,
                            () => OpenAddItem(TuiScreen.Review));
                        break;

                    case TuiScreen.Search:
                        SearchScreen.Build(
                            root,
                            searchVm,
                            navigation,
                            NavigateBack,
                            Quit,
                            async query =>
                            {
                                if (searchItemsUseCase is null || string.IsNullOrWhiteSpace(query))
                                {
                                    return;
                                }

                                var results = await searchItemsUseCase.ExecuteAsync(
                                    new SearchItemsRequest { Query = query },
                                    cancellationToken);
                                TGui.MainLoop?.Invoke(() =>
                                {
                                    searchVm.SetResults(results);
                                    ScheduleRender();
                                });
                            });
                        break;

                    case TuiScreen.Config:
                        ConfigScreen.Build(
                            root,
                            new ConfigViewModel(snapshot.Configuration),
                            navigation,
                            NavigateBack,
                            Quit);
                        break;

                    case TuiScreen.AddItem:
                        var addItemVm = TuiAddItemViewModel.ForSourceScreen(addSourceScreen);
                        if (addError is not null)
                        {
                            addItemVm = addItemVm.WithError(addError);
                        }

                        AddItemScreen.Build(
                            root,
                            addItemVm,
                            rawInput =>
                            {
                                _ = Task.Run(async () =>
                                {
                                    try
                                    {
                                        if (createItemUseCase is null)
                                        {
                                            throw new InvalidOperationException("Add item is not available.");
                                        }

                                        var request = QuickCaptureParser.Parse(rawInput, addItemVm.Collection);
                                        await createItemUseCase.ExecuteAsync(request, cancellationToken);
                                        var refreshed = await snapshotLoader.LoadAsync(cancellationToken);
                                        TGui.MainLoop?.Invoke(() =>
                                        {
                                            snapshot = refreshed;
                                            addError = null;
                                            navigation.NavigateBack();
                                            ScheduleRender();
                                        });
                                    }
                                    catch (Exception ex) when (ex is not OperationCanceledException)
                                    {
                                        TGui.MainLoop?.Invoke(() =>
                                        {
                                            addError = ex.Message;
                                            ScheduleRender();
                                        });
                                    }
                                }, cancellationToken);
                            },
                            () =>
                            {
                                addError = null;
                                NavigateBack();
                            },
                            Quit);
                        break;

                    default:
                        BuildMainDashboard(
                            root,
                            dashboardVm,
                            navigation,
                            actionHandler,
                            createItemUseCase,
                            screen => NavigateTo(screen, GetPanelCount(screen)),
                            () => OpenAddItem(TuiScreen.MainDashboard),
                            RefreshAndRender,
                            Quit,
                            cancellationToken);
                        break;
                }
            }

            Render();
            TGui.Run(top);
        }
        finally
        {
            TGui.RootKeyEvent = null;
            TGui.Shutdown();
        }
    }

    private static void BuildMainDashboard(
        View root,
        MainDashboardViewModel viewModel,
        TuiNavigationState navigation,
        MainDashboardActionHandler? actionHandler,
        CreateItemUseCase? createItemUseCase,
        Action<TuiScreen> onNavigate,
        Action onAdd,
        Action onRefresh,
        Action onQuit,
        CancellationToken cancellationToken)
    {
        var date = DateTime.Today.ToString("yyyy-MM-dd");
        var topBar = new Label($" TermBullet \u2500 Daily {date} \u2500 data:local \u2500 ai:off \u2500 sync:idle \u2500 mode:normal")
        {
            X = 0, Y = 0, Width = Dim.Fill()
        };

        var footer = new Label(" / filter  c add  e edit  x done  > migrate  Enter zoom  Tab focus  ? help  q quit")
        {
            X = 0, Y = Pos.AnchorEnd(1), Width = Dim.Fill()
        };

        var upperHeight = Dim.Percent(55);
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

        var dayItemsPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(2, "Day Items", navigation, 1))
        {
            X = Pos.Right(collectionsPanel), Y = 1, Width = Dim.Percent(50), Height = upperHeight
        };
        var dayItemsList = new ListView(
            viewModel.DayItems.Count > 0
                ? viewModel.DayItems.Select(r => $"{r.Symbol} {r.PublicRef} {r.Content}").ToArray()
                : ["(no items)"])
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };
        if (viewModel.SelectedDayItemIndex >= 0)
        {
            dayItemsList.SelectedItem = viewModel.SelectedDayItemIndex;
        }
        dayItemsPanel.Add(dayItemsList);

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

        root.Add(topBar, collectionsPanel, dayItemsPanel, previewPanel, projectsPanel, filteredBacklogPanel, suggestedPlanPanel, footer);

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

        root.KeyPress += args =>
        {
            if (TuiScreenUtilities.IsHelpKey(args.KeyEvent))
            {
                TuiScreenUtilities.ShowContextHelp(TuiScreen.MainDashboard);
                args.Handled = true;
                return;
            }

            switch (args.KeyEvent.Key)
            {
                case Key.q:
                    onQuit();
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
                    NavigateFromCollections(collectionsList.SelectedItem, onNavigate);
                    args.Handled = true;
                    break;
                case Key x when x == (Key)'/' :
                    onNavigate(TuiScreen.Search);
                    args.Handled = true;
                    break;
                case Key x when x == (Key)'c' && createItemUseCase is not null:
                    onAdd();
                    args.Handled = true;
                    break;
                case Key x when x == (Key)'x' && actionHandler is not null:
                    DispatchAction(viewModel.SelectedDayItem?.PublicRef, actionHandler.HandleDoneAsync, onRefresh, cancellationToken);
                    args.Handled = true;
                    break;
                case Key y when y == (Key)'>' && actionHandler is not null:
                    DispatchAction(viewModel.SelectedDayItem?.PublicRef, actionHandler.HandleMigrateAsync, onRefresh, cancellationToken);
                    args.Handled = true;
                    break;
                case Key z when z == (Key)'d' && actionHandler is not null:
                    DispatchAction(viewModel.SelectedDayItem?.PublicRef, actionHandler.HandleDeleteAsync, onRefresh, cancellationToken);
                    args.Handled = true;
                    break;
            }
        };
    }

    private static void NavigateFromCollections(int selectedIndex, Action<TuiScreen> onNavigate)
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
            onNavigate(screen.Value);
        }
    }

    private static int GetPanelCount(TuiScreen screen) =>
        screen switch
        {
            TuiScreen.DailyFocus => 6,
            TuiScreen.WeeklyPlanning => 6,
            TuiScreen.Review => 6,
            TuiScreen.Search => 2,
            TuiScreen.Config => 3,
            TuiScreen.AddItem => 1,
            _ => 3
        };

    private static void DispatchAction(
        string? publicRef,
        Func<string, CancellationToken, Task<ActionResult>> handler,
        Action onRefresh,
        CancellationToken cancellationToken)
    {
        if (publicRef is null) return;
        _ = Task.Run(async () =>
        {
            await handler(publicRef, cancellationToken);
            TGui.MainLoop?.Invoke(onRefresh);
        }, cancellationToken);
    }

    private static string[] BuildPreviewLines(ItemDisplayRow? item) =>
        item is not null
            ?
            [
                item.PublicRef,
                $"type: {item.Type}",
                $"status: {item.Status}",
                $"priority: {item.Priority}",
                $"collection: {item.Collection}",
                $"tags: {(item.Tags.Length > 0 ? string.Join(", ", item.Tags) : "(none)")}"
            ]
            : ["(nothing selected)"];
}
