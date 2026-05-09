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
        var navigation = new TuiNavigationState(panelCount: 5);
        var auxiliaryFlow = TuiAuxiliaryFlow.None;
        ItemDisplayRow? selectedItem = null;
        MigrateItemViewModel? migrateItemVm = null;
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

            void OpenAddItem()
            {
                addError = null;
                auxiliaryFlow = TuiAuxiliaryFlow.AddItem;
                ScheduleRender();
            }

            void OpenItemDetail(ItemDisplayRow? item)
            {
                if (item is null) return;
                selectedItem = item;
                NavigateTo(TuiScreen.ItemDetail, GetPanelCount(TuiScreen.ItemDetail));
            }

            void OpenMigrateItem(ItemDisplayRow? item)
            {
                if (item is null) return;
                selectedItem = item;
                migrateItemVm = MigrateItemViewModel.ForDate(
                    item,
                    DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
                NavigateTo(TuiScreen.MigrateItem, GetPanelCount(TuiScreen.MigrateItem));
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
                if (auxiliaryFlow == TuiAuxiliaryFlow.AddItem)
                {
                    if (keyEvent.Key == Key.Esc)
                    {
                        addError = null;
                        auxiliaryFlow = TuiAuxiliaryFlow.None;
                        ScheduleRender();
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
                    OpenAddItem();
                    return true;
                }

                if (keyEvent.Key == (Key)'/' && navigation.CurrentScreen == TuiScreen.MainDashboard)
                {
                    NavigateTo(TuiScreen.Search, GetPanelCount(TuiScreen.Search));
                    return true;
                }

                if (selectedItem is not null
                    && actionHandler is not null
                    && TuiItemActionShortcutMapper.TryMap(keyEvent.Key, out var action))
                {
                    if (action == TuiItemActionShortcut.Migrate)
                    {
                        OpenMigrateItem(selectedItem);
                        return true;
                    }

                    Func<string, CancellationToken, Task<ActionResult>>? handler = action switch
                    {
                        TuiItemActionShortcut.Done => actionHandler.HandleDoneAsync,
                        TuiItemActionShortcut.Cancel => actionHandler.HandleCancelAsync,
                        TuiItemActionShortcut.Delete => actionHandler.HandleDeleteAsync,
                        _ => null
                    };

                    if (handler is not null)
                    {
                        DispatchAction(selectedItem.PublicRef, handler, RefreshAndRender, cancellationToken);
                        return true;
                    }
                }

                return false;
            };

            void Render()
            {
                var root = host.ReplaceContent();
                var dashboardVm = new MainDashboardViewModel(snapshot.TodayItems, snapshot.BacklogItems);

                if (auxiliaryFlow == TuiAuxiliaryFlow.AddItem)
                {
                    var addItemVm = TuiAddItemViewModel.ForMainDashboard();
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
                                        auxiliaryFlow = TuiAuxiliaryFlow.None;
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
                            auxiliaryFlow = TuiAuxiliaryFlow.None;
                            ScheduleRender();
                        },
                        Quit);
                    return;
                }

                switch (navigation.CurrentScreen)
                {
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
                            },
                            OpenItemDetail);
                        break;

                    case TuiScreen.ItemDetail when selectedItem is not null:
                        ItemDetailScreen.Build(
                            root,
                            ItemDetailViewModel.FromRow(selectedItem),
                            navigation,
                            NavigateBack,
                            () => OpenMigrateItem(selectedItem),
                            Quit);
                        break;

                    case TuiScreen.MigrateItem when selectedItem is not null && migrateItemVm is not null:
                        MigrateItemScreen.Build(
                            root,
                            migrateItemVm,
                            navigation,
                            updated =>
                            {
                                migrateItemVm = updated;
                                ScheduleRender();
                            },
                            () =>
                            {
                                if (actionHandler is null) return;
                                DispatchAction(
                                    selectedItem.PublicRef,
                                    actionHandler.HandleMigrateAsync,
                                    () =>
                                    {
                                        RefreshAndRender();
                                        NavigateBack();
                                    },
                                    cancellationToken);
                            },
                            NavigateBack);
                        break;

                    default:
                        BuildMainDashboard(
                            root,
                            dashboardVm,
                            navigation,
                            actionHandler,
                            createItemUseCase,
                            item => selectedItem = item,
                            screen => NavigateTo(screen, GetPanelCount(screen)),
                            OpenItemDetail,
                            OpenMigrateItem,
                            OpenAddItem,
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
        Action<ItemDisplayRow?> onSelectedItemChanged,
        Action<TuiScreen> onNavigate,
        Action<ItemDisplayRow?> onOpenDetail,
        Action<ItemDisplayRow?> onOpenMigrate,
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

        var footer = new Label(" / filter  c add  e edit  x done  z cancel  > migrate  d delete  Enter open  Tab focus  ? help  q quit")
        {
            X = 0, Y = Pos.AnchorEnd(1), Width = Dim.Fill()
        };

        var upperHeight = Dim.Percent(55);
        var menuPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(1, "Menu", navigation, 0))
        {
            X = 0, Y = 1, Width = Dim.Percent(20), Height = upperHeight
        };
        var menuEntries = new[] { "> Dashboard", "  Search", "  Planning", "  Calendar" };
        var menuList = new ListView(TuiScreenUtilities.SanitizeListItems(menuEntries))
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };
        menuPanel.Add(menuList);

        var dayItemsPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(2, "Day Items", navigation, 1))
        {
            X = Pos.Right(menuPanel), Y = 1, Width = Dim.Percent(50), Height = upperHeight
        };
        var dayItemsList = new ListView(TuiScreenUtilities.SanitizeListItems(
            viewModel.DayItems.Count > 0
                ? viewModel.DayItems.Select(r => $"{r.Symbol} {r.PublicRef} {r.Content}").ToArray()
                : ["(no items)"]))
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };
        if (viewModel.SelectedDayItemIndex >= 0)
        {
            dayItemsList.SelectedItem = viewModel.SelectedDayItemIndex;
        }
        dayItemsPanel.Add(dayItemsList);

        var detailsPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(3, "Details", navigation, 2))
        {
            X = Pos.Right(dayItemsPanel), Y = 1, Width = Dim.Fill(), Height = upperHeight
        };
        var detailsList = new ListView(TuiScreenUtilities.SanitizeListItems(BuildPreviewLines(viewModel.SelectedDayItem)))
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };
        detailsPanel.Add(detailsList);

        var contextPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(4, "Context", navigation, 3))
        {
            X = 0, Y = Pos.Bottom(menuPanel), Width = Dim.Percent(20), Height = Dim.Fill(1)
        };
        var contextRows = BuildContextLines(viewModel);
        var contextList = new ListView(TuiScreenUtilities.SanitizeListItems(contextRows))
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };
        contextPanel.Add(contextList);

        var contentPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(5, "Content", navigation, 4))
        {
            X = Pos.Right(contextPanel), Y = Pos.Bottom(dayItemsPanel), Width = Dim.Fill(), Height = Dim.Fill(1)
        };
        var contentList = new ListView(TuiScreenUtilities.SanitizeListItems(BuildContentLines(viewModel.SelectedDayItem)))
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };
        contentPanel.Add(contentList);

        var panels = new[]
        {
            menuPanel, dayItemsPanel, detailsPanel, contextPanel, contentPanel
        };
        var panelTitles = new[]
        {
            "Menu", "Day Items", "Details", "Context", "Content"
        };
        var focusTargets = new View[]
        {
            menuList, dayItemsList, detailsList, contextList, contentList
        };
        TuiScreenUtilities.UpdatePanelTitles(panels, panelTitles, navigation);
        TuiScreenUtilities.FocusCurrentPanel(focusTargets, navigation);
        onSelectedItemChanged(viewModel.SelectedDayItem);

        root.Add(topBar, menuPanel, dayItemsPanel, detailsPanel, contextPanel, contentPanel, footer);

        dayItemsList.SelectedItemChanged += _ =>
        {
            var newIndex = dayItemsList.SelectedItem;
            if (newIndex < 0 || newIndex >= viewModel.DayItems.Count) return;
            var diff = newIndex - viewModel.SelectedDayItemIndex;
            if (diff > 0)
                for (var i = 0; i < diff; i++) viewModel.SelectNextDayItem();
            else if (diff < 0)
                for (var i = 0; i < -diff; i++) viewModel.SelectPreviousDayItem();

            TuiScreenUtilities.RefreshListView(detailsList, BuildPreviewLines(viewModel.SelectedDayItem));
            TuiScreenUtilities.RefreshListView(contentList, BuildContentLines(viewModel.SelectedDayItem));
            onSelectedItemChanged(viewModel.SelectedDayItem);
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
                    if (navigation.FocusedPanelIndex == 0)
                    {
                        NavigateFromMenu(menuList.SelectedItem, onNavigate);
                    }
                    else
                    {
                        onOpenDetail(viewModel.SelectedDayItem);
                    }
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
                    onOpenMigrate(viewModel.SelectedDayItem);
                    args.Handled = true;
                    break;
                case Key z when z == (Key)'d' && actionHandler is not null:
                    DispatchAction(viewModel.SelectedDayItem?.PublicRef, actionHandler.HandleDeleteAsync, onRefresh, cancellationToken);
                    args.Handled = true;
                    break;
            }
        };
    }

    private static void NavigateFromMenu(int selectedIndex, Action<TuiScreen> onNavigate)
    {
        var screen = selectedIndex switch
        {
            1 => TuiScreen.Search,
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
            TuiScreen.Search => 2,
            TuiScreen.ItemDetail => 5,
            TuiScreen.MigrateItem => 3,
            _ => 5
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
                $"ref: {item.PublicRef}",
                $"type: {item.Type}",
                $"status: {item.Status}",
                $"priority: {item.Priority}",
                $"collection: {item.Collection}",
                $"tags: {(item.Tags.Length > 0 ? string.Join(", ", item.Tags) : "(none)")}"
            ]
            : ["(nothing selected)"];

    private static string[] BuildContextLines(MainDashboardViewModel viewModel)
    {
        var lines = new List<string>
        {
            "collections",
            $"> today      {viewModel.DayItems.Count}",
            "  week view  -",
            $"  backlog    {viewModel.BacklogItems.Count}",
            "  forgotten  -",
            "tags"
        };

        if (viewModel.ProjectOrTagRows.Count == 0)
        {
            lines.Add("> (none)");
        }
        else
        {
            lines.Add($"> {string.Join("  ", viewModel.ProjectOrTagRows)}");
        }

        return [.. lines];
    }

    private static string[] BuildContentLines(ItemDisplayRow? item)
    {
        if (item is null)
        {
            return ["(nothing selected)"];
        }

        return
        [
            item.Content,
            " ",
            "Description:",
            string.IsNullOrWhiteSpace(item.Description) ? "-" : item.Description
        ];
    }
}
