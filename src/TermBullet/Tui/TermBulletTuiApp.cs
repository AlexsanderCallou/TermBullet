using Terminal.Gui;
using TermBullet.Application.Items;
using TermBullet.Application.Tags;
using TermBullet.Domain.Items;
using TermBullet.Tui.Navigation;
using TermBullet.Tui.Screens;
using TGui = Terminal.Gui.Application;

namespace TermBullet.Tui;

public sealed class TermBulletTuiApp(
    GetTodayItemsUseCase getTodayItemsUseCase,
    GetBacklogItemsUseCase getBacklogItemsUseCase,
    GetWeekItemsUseCase? getWeekItemsUseCase = null,
    GetMonthItemsUseCase? getMonthItemsUseCase = null,
    ListItemsUseCase? listItemsUseCase = null,
    SearchItemsUseCase? searchItemsUseCase = null,
    ListTagsUseCase? listTagsUseCase = null,
    CreateTagUseCase? createTagUseCase = null,
    CreateItemUseCase? createItemUseCase = null,
    EditItemUseCase? editItemUseCase = null,
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
            getMonthItemsUseCase,
            getBacklogItemsUseCase,
            listItemsUseCase,
            listTagsUseCase,
            startupAction);
        var snapshot = await snapshotLoader.LoadAsync(cancellationToken);
        var searchVm = new SearchViewModel();
        var navigation = new TuiNavigationState(panelCount: 5);
        var auxiliaryFlow = TuiAuxiliaryFlow.None;
        var addItemType = ItemType.Task;
        ItemDisplayRow? selectedItem = null;
        MigrateItemViewModel? migrateItemVm = null;
        string? addError = null;
        string? editError = null;
        string? createTagError = null;

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
                auxiliaryFlow = TuiAuxiliaryFlow.AddItemTypePicker;
                ScheduleRender();
            }

            void OpenAddItemForm(ItemType type)
            {
                addItemType = type;
                addError = null;
                auxiliaryFlow = TuiAuxiliaryFlow.AddItem;
                ScheduleRender();
            }

            void OpenQuickTask()
            {
                addError = null;
                auxiliaryFlow = TuiAuxiliaryFlow.QuickTask;
                ScheduleRender();
            }

            void OpenEditItem(ItemDisplayRow? item)
            {
                if (item is null || editItemUseCase is null) return;
                selectedItem = item;
                editError = null;
                auxiliaryFlow = TuiAuxiliaryFlow.EditItem;
                ScheduleRender();
            }

            void OpenCreateTag()
            {
                createTagError = null;
                auxiliaryFlow = TuiAuxiliaryFlow.CreateTag;
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
                if (!item.Type.Equals("task", StringComparison.OrdinalIgnoreCase)) return;
                selectedItem = item;
                migrateItemVm = MigrateItemViewModel.ForCollection(item, TermBullet.Domain.Items.ItemCollection.Today);
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

            void DispatchSelectedAction(
                ItemDisplayRow? item,
                Func<string, CancellationToken, Task<ActionResult>>? handler)
            {
                if (item is null || handler is null)
                {
                    return;
                }

                DispatchAction(item.PublicRef, handler, RefreshAndRender, cancellationToken);
            }

            void SubmitCreateRequest(CreateItemRequest request)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        if (createItemUseCase is null)
                        {
                            throw new InvalidOperationException("Add item is not available.");
                        }

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
            }

            void SubmitEditRequest(EditItemRequest request)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        if (editItemUseCase is null)
                        {
                            throw new InvalidOperationException("Edit item is not available.");
                        }

                        var item = await editItemUseCase.ExecuteAsync(request, cancellationToken);
                        var refreshed = await snapshotLoader.LoadAsync(cancellationToken);
                        TGui.MainLoop?.Invoke(() =>
                        {
                            snapshot = refreshed;
                            selectedItem = ItemDisplayRow.From(item);
                            editError = null;
                            auxiliaryFlow = TuiAuxiliaryFlow.None;
                            ScheduleRender();
                        });
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        TGui.MainLoop?.Invoke(() =>
                        {
                            editError = ex.Message;
                            ScheduleRender();
                        });
                    }
                }, cancellationToken);
            }

            TGui.RootKeyEvent = keyEvent =>
            {
                if (auxiliaryFlow != TuiAuxiliaryFlow.None)
                {
                    if (keyEvent.Key == Key.Esc)
                    {
                        addError = null;
                        editError = null;
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

                if (keyEvent.Key == (Key)'c'
                    && navigation.CurrentScreen == TuiScreen.MainDashboard
                    && createItemUseCase is not null)
                {
                    OpenAddItem();
                    return true;
                }

                if (keyEvent.Key == (Key)'n'
                    && navigation.CurrentScreen == TuiScreen.MainDashboard
                    && createItemUseCase is not null)
                {
                    OpenQuickTask();
                    return true;
                }

                if (keyEvent.Key == (Key)'/' && navigation.CurrentScreen == TuiScreen.MainDashboard)
                {
                    NavigateTo(TuiScreen.Search, GetPanelCount(TuiScreen.Search));
                    return true;
                }

                if (keyEvent.Key == (Key)'e' && selectedItem is not null && editItemUseCase is not null)
                {
                    OpenEditItem(selectedItem);
                    return true;
                }

                if (selectedItem is not null
                    && actionHandler is not null
                    && navigation.CurrentScreen == TuiScreen.ItemDetail
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
                var dashboardVm = new MainDashboardViewModel(
                    snapshot.TodayItems,
                    snapshot.WeekItems,
                    snapshot.MonthItems,
                    snapshot.BacklogItems);

                if (auxiliaryFlow == TuiAuxiliaryFlow.AddItemTypePicker)
                {
                    AddItemTypePickerScreen.Build(
                        root,
                        OpenAddItemForm,
                        () =>
                        {
                            addError = null;
                            auxiliaryFlow = TuiAuxiliaryFlow.None;
                            ScheduleRender();
                        });
                    return;
                }

                if (auxiliaryFlow == TuiAuxiliaryFlow.QuickTask)
                {
                    QuickTaskScreen.Build(
                        root,
                        addError,
                        SubmitCreateRequest,
                        () =>
                        {
                            addError = null;
                            auxiliaryFlow = TuiAuxiliaryFlow.None;
                            ScheduleRender();
                        });
                    return;
                }

                if (auxiliaryFlow == TuiAuxiliaryFlow.AddItem)
                {
                    var addItemVm = TuiAddItemViewModel.ForType(addItemType);
                    if (addError is not null)
                    {
                        addItemVm = addItemVm.WithError(addError);
                    }

                    AddItemScreen.Build(
                        root,
                        addItemVm,
                        snapshot.Tags.Select(tag => tag.Name).ToArray(),
                        SubmitCreateRequest,
                        () =>
                        {
                            addError = null;
                            auxiliaryFlow = TuiAuxiliaryFlow.None;
                            ScheduleRender();
                        },
                        Quit);
                    return;
                }

                if (auxiliaryFlow == TuiAuxiliaryFlow.EditItem && selectedItem is not null)
                {
                    EditItemScreen.Build(
                        root,
                        EditItemFormDraft.FromRow(selectedItem),
                        snapshot.Tags.Select(tag => tag.Name).ToArray(),
                        editError,
                        SubmitEditRequest,
                        () =>
                        {
                            editError = null;
                            auxiliaryFlow = TuiAuxiliaryFlow.None;
                            ScheduleRender();
                        },
                        Quit);
                    return;
                }

                if (auxiliaryFlow == TuiAuxiliaryFlow.CreateTag)
                {
                    CreateTagScreen.Build(
                        root,
                        createTagError,
                        (name, description) =>
                        {
                            _ = Task.Run(async () =>
                            {
                                try
                                {
                                    if (createTagUseCase is null)
                                    {
                                        throw new InvalidOperationException("Tag creation is not available.");
                                    }

                                    await createTagUseCase.ExecuteAsync(
                                        new CreateTagRequest { Name = name, Description = string.IsNullOrWhiteSpace(description) ? null : description },
                                        cancellationToken);
                                    var refreshed = await snapshotLoader.LoadAsync(cancellationToken);
                                    TGui.MainLoop?.Invoke(() =>
                                    {
                                        snapshot = refreshed;
                                        createTagError = null;
                                        auxiliaryFlow = TuiAuxiliaryFlow.None;
                                        ScheduleRender();
                                    });
                                }
                                catch (Exception ex) when (ex is not OperationCanceledException)
                                {
                                    TGui.MainLoop?.Invoke(() =>
                                    {
                                        createTagError = ex.Message;
                                        ScheduleRender();
                                    });
                                }
                            }, cancellationToken);
                        },
                        () =>
                        {
                            createTagError = null;
                            auxiliaryFlow = TuiAuxiliaryFlow.None;
                            ScheduleRender();
                        });
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
                            item => selectedItem = item,
                            OpenEditItem,
                            OpenItemDetail);
                        break;

                    case TuiScreen.ItemDetail when selectedItem is not null:
                        ItemDetailScreen.Build(
                            root,
                            ItemDetailViewModel.FromRow(selectedItem),
                            navigation,
                            NavigateBack,
                            () => OpenEditItem(selectedItem),
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
                            updated =>
                            {
                                if (actionHandler is null) return;
                                _ = Task.Run(async () =>
                                {
                                    await actionHandler.HandleMigrateAsync(
                                        updated.BuildRequest(),
                                        cancellationToken);
                                    TGui.MainLoop?.Invoke(() =>
                                    {
                                        RefreshAndRender();
                                        NavigateBack();
                                    });
                                }, cancellationToken);
                            },
                            NavigateBack);
                        break;

                    case TuiScreen.Planning:
                        PlanningScreen.Build(root, NavigateBack, Quit);
                        break;

                    case TuiScreen.Week:
                        ItemListScreen.Build(
                            root,
                            "Week",
                            snapshot.WeekItems.Select(ItemDisplayRow.From).ToArray(),
                            "Actions",
                            ["> migrate selected task", "Enter open detail", "x mark done", "z cancel", "d delete"],
                            item => selectedItem = item,
                            OpenItemDetail,
                            OpenEditItem,
                            OpenMigrateItem,
                            item => { if (actionHandler is not null) DispatchSelectedAction(item, actionHandler.HandleDoneAsync); },
                            item => { if (actionHandler is not null) DispatchSelectedAction(item, actionHandler.HandleCancelAsync); },
                            item => { if (actionHandler is not null) DispatchSelectedAction(item, actionHandler.HandleDeleteAsync); },
                            NavigateBack,
                            Quit,
                            row => $"{row.Symbol} {row.PublicRef} {row.Content}".Trim(),
                            " Enter open  e edit  > migrate  x done  z cancel  d delete  Tab/1-3 focus  ? help  Esc back  q quit");
                        break;

                    case TuiScreen.Month:
                        ItemListScreen.Build(
                            root,
                            "Month",
                            snapshot.MonthItems.Select(ItemDisplayRow.From).ToArray(),
                            "Actions",
                            ["> migrate selected task", "Enter open detail", "x mark done", "z cancel", "d delete"],
                            item => selectedItem = item,
                            OpenItemDetail,
                            OpenEditItem,
                            OpenMigrateItem,
                            item => { if (actionHandler is not null) DispatchSelectedAction(item, actionHandler.HandleDoneAsync); },
                            item => { if (actionHandler is not null) DispatchSelectedAction(item, actionHandler.HandleCancelAsync); },
                            item => { if (actionHandler is not null) DispatchSelectedAction(item, actionHandler.HandleDeleteAsync); },
                            NavigateBack,
                            Quit,
                            row => $"{row.Symbol} {row.PublicRef} {row.Content}".Trim(),
                            " Enter open  e edit  > migrate  x done  z cancel  d delete  Tab/1-3 focus  ? help  Esc back  q quit");
                        break;

                    case TuiScreen.Backlog:
                        ItemListScreen.Build(
                            root,
                            "Backlog",
                            snapshot.BacklogItems.Select(ItemDisplayRow.From).ToArray(),
                            "Actions",
                            ["> migrate selected task", "Enter open detail", "x mark done", "z cancel", "d delete"],
                            item => selectedItem = item,
                            OpenItemDetail,
                            OpenEditItem,
                            OpenMigrateItem,
                            item => { if (actionHandler is not null) DispatchSelectedAction(item, actionHandler.HandleDoneAsync); },
                            item => { if (actionHandler is not null) DispatchSelectedAction(item, actionHandler.HandleCancelAsync); },
                            item => { if (actionHandler is not null) DispatchSelectedAction(item, actionHandler.HandleDeleteAsync); },
                            NavigateBack,
                            Quit,
                            row => $"{row.Symbol} {row.PublicRef} {row.Content}".Trim(),
                            " Enter open  e edit  > migrate  x done  z cancel  d delete  Tab/1-3 focus  ? help  Esc back  q quit");
                        break;

                    case TuiScreen.Forgotten:
                        ItemListScreen.Build(
                            root,
                            "Forgotten",
                            BuildForgottenRows(snapshot),
                            "Resolution",
                            ["> migrate selected task", "x mark done", "z cancel", "d delete"],
                            item => selectedItem = item,
                            OpenItemDetail,
                            OpenEditItem,
                            OpenMigrateItem,
                            item => { if (actionHandler is not null) DispatchSelectedAction(item, actionHandler.HandleDoneAsync); },
                            item => { if (actionHandler is not null) DispatchSelectedAction(item, actionHandler.HandleCancelAsync); },
                            item => { if (actionHandler is not null) DispatchSelectedAction(item, actionHandler.HandleDeleteAsync); },
                            NavigateBack,
                            Quit,
                            FormatForgottenRow);
                        break;

                    case TuiScreen.Notes:
                        ItemListScreen.Build(
                            root,
                            "Notes",
                            CalendarViewModel.BuildNoteRows(snapshot.AllItems.Select(ItemDisplayRow.From).ToArray()),
                            "Actions",
                            ["> open detail", "d delete"],
                            item => selectedItem = item,
                            OpenItemDetail,
                            OpenEditItem,
                            _ => { },
                            _ => { },
                            _ => { },
                            item => { if (actionHandler is not null) DispatchSelectedAction(item, actionHandler.HandleDeleteAsync); },
                            NavigateBack,
                            Quit,
                            row => $"{row.Symbol} {row.PublicRef} {row.Content}".Trim(),
                            " Enter open  e edit  d delete  Tab/1-3 focus  ? help  Esc back  q quit");
                        break;

                    case TuiScreen.Calendar:
                        CalendarScreen.Build(
                            root,
                            snapshot.CurrentItems.Select(ItemDisplayRow.From).ToArray(),
                            item => selectedItem = item,
                            OpenItemDetail,
                            OpenEditItem,
                            OpenMigrateItem,
                            item => { if (actionHandler is not null) DispatchSelectedAction(item, actionHandler.HandleDoneAsync); },
                            item => { if (actionHandler is not null) DispatchSelectedAction(item, actionHandler.HandleCancelAsync); },
                            item => { if (actionHandler is not null) DispatchSelectedAction(item, actionHandler.HandleDeleteAsync); },
                            NavigateBack,
                            Quit);
                        break;

                    case TuiScreen.Tags:
                        selectedItem = null;
                        TagsScreen.Build(
                            root,
                            TagsViewModel.Build(
                                snapshot.Tags,
                                snapshot.AllItems.Select(ItemDisplayRow.From).ToArray()),
                            OpenCreateTag,
                            NavigateBack,
                            Quit);
                        break;

                    default:
                        BuildMainDashboard(
                            root,
                            dashboardVm,
                            BuildForgottenRows(snapshot).Length,
                            navigation,
                            actionHandler,
                            createItemUseCase,
                            item => selectedItem = item,
                            screen => NavigateTo(screen, GetPanelCount(screen)),
                            OpenItemDetail,
                            OpenEditItem,
                            OpenMigrateItem,
                            OpenAddItem,
                            OpenQuickTask,
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
        int forgottenCount,
        TuiNavigationState navigation,
        MainDashboardActionHandler? actionHandler,
        CreateItemUseCase? createItemUseCase,
        Action<ItemDisplayRow?> onSelectedItemChanged,
        Action<TuiScreen> onNavigate,
        Action<ItemDisplayRow?> onOpenDetail,
        Action<ItemDisplayRow?> onOpenEdit,
        Action<ItemDisplayRow?> onOpenMigrate,
        Action onAdd,
        Action onQuickTask,
        Action onRefresh,
        Action onQuit,
        CancellationToken cancellationToken)
    {
        var date = DateTime.Today.ToString("yyyy-MM-dd");
        var topBar = new Label($" TermBullet - Daily {date}")
        {
            X = 0, Y = 0, Width = Dim.Fill()
        };

        var footer = new Label(" / filter  c add  n quick task  e edit  x done  z cancel  > migrate  d delete  Enter open  Tab/1-5 focus  ? help  q quit")
        {
            X = 0, Y = Pos.AnchorEnd(1), Width = Dim.Fill()
        };

        var upperHeight = Dim.Percent(55);
        var menuPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(1, "Menu", navigation, 0))
        {
            X = 0, Y = 1, Width = Dim.Percent(20), Height = upperHeight
        };
        var menuEntries = new[]
        {
            "> Dashboard",
            "  Search",
            "  Planning",
            "  Month",
            "  Backlog",
            "  Forgotten",
            "  Notes",
            "  Calendar",
            "  Tags"
        };
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
        var contextRows = BuildContextLines(viewModel, forgottenCount);
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
        menuList.KeyPress += args =>
        {
            if (TuiScreenUtilities.TryHandleEnter(args.KeyEvent.Key, () => NavigateFromMenu(menuList.SelectedItem, onNavigate)))
            {
                args.Handled = true;
            }
        };
        contextList.KeyPress += args =>
        {
            if (TuiScreenUtilities.TryHandleEnter(args.KeyEvent.Key, () => NavigateFromContext(contextList.SelectedItem, onNavigate)))
            {
                args.Handled = true;
            }
        };
        dayItemsList.KeyPress += args =>
        {
            if (TuiScreenUtilities.TryHandleEnter(args.KeyEvent.Key, () => onOpenDetail(viewModel.SelectedDayItem)))
            {
                args.Handled = true;
            }
        };

        bool HandleDashboardKey(KeyEvent keyEvent, bool includeEnter)
        {
            if (TuiScreenUtilities.IsHelpKey(keyEvent))
            {
                TuiScreenUtilities.ShowContextHelp(TuiScreen.MainDashboard);
                return true;
            }

            if (TuiScreenUtilities.TryFocusPanelByNumber(keyEvent, navigation, panels, panelTitles, focusTargets))
            {
                return true;
            }

            switch (keyEvent.Key)
            {
                case Key.q:
                    onQuit();
                    return true;
                case Key.Tab:
                    navigation.MoveNextPanel();
                    TuiScreenUtilities.UpdatePanelTitles(panels, panelTitles, navigation);
                    TuiScreenUtilities.FocusCurrentPanel(focusTargets, navigation);
                    return true;
                case Key.BackTab:
                    navigation.MovePreviousPanel();
                    TuiScreenUtilities.UpdatePanelTitles(panels, panelTitles, navigation);
                    TuiScreenUtilities.FocusCurrentPanel(focusTargets, navigation);
                    return true;
                case Key.Enter when includeEnter:
                    if (navigation.FocusedPanelIndex == 0)
                    {
                        NavigateFromMenu(menuList.SelectedItem, onNavigate);
                    }
                    else if (navigation.FocusedPanelIndex == 3)
                    {
                        NavigateFromContext(contextList.SelectedItem, onNavigate);
                    }
                    else
                    {
                        onOpenDetail(viewModel.SelectedDayItem);
                    }
                    return true;
                case Key x when x == (Key)'/' :
                    onNavigate(TuiScreen.Search);
                    return true;
                case Key x when x == (Key)'c' && createItemUseCase is not null:
                    onAdd();
                    return true;
                case Key n when n == (Key)'n' && createItemUseCase is not null:
                    onQuickTask();
                    return true;
                case Key e when e == (Key)'e':
                    onOpenEdit(viewModel.SelectedDayItem);
                    return true;
                case Key x when x == (Key)'x' && actionHandler is not null:
                    DispatchAction(viewModel.SelectedDayItem?.PublicRef, actionHandler.HandleDoneAsync, onRefresh, cancellationToken);
                    return true;
                case Key z when z == (Key)'z' && actionHandler is not null:
                    DispatchAction(viewModel.SelectedDayItem?.PublicRef, actionHandler.HandleCancelAsync, onRefresh, cancellationToken);
                    return true;
                case Key y when y == (Key)'>' && actionHandler is not null:
                    onOpenMigrate(viewModel.SelectedDayItem);
                    return true;
                case Key z when z == (Key)'d' && actionHandler is not null:
                    DispatchAction(viewModel.SelectedDayItem?.PublicRef, actionHandler.HandleDeleteAsync, onRefresh, cancellationToken);
                    return true;
            }

            return false;
        }

        root.KeyPress += args =>
        {
            if (HandleDashboardKey(args.KeyEvent, includeEnter: true))
            {
                args.Handled = true;
            }
        };

        foreach (var target in focusTargets)
        {
            target.KeyPress += args =>
            {
                if (HandleDashboardKey(args.KeyEvent, includeEnter: false))
                {
                    args.Handled = true;
                }
            };
        }
    }

    private static void NavigateFromMenu(int selectedIndex, Action<TuiScreen> onNavigate)
    {
        var screen = DashboardNavigationMapper.FromMenuIndex(selectedIndex);

        if (screen.HasValue)
        {
            onNavigate(screen.Value);
        }
    }

    private static void NavigateFromContext(int selectedIndex, Action<TuiScreen> onNavigate)
    {
        var screen = DashboardNavigationMapper.FromContextIndex(selectedIndex);

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
            TuiScreen.MigrateItem => 1,
            TuiScreen.Planning => 1,
            TuiScreen.Week => 3,
            TuiScreen.Month => 3,
            TuiScreen.Backlog => 3,
            TuiScreen.Forgotten => 3,
            TuiScreen.Notes => 3,
            TuiScreen.Calendar => 4,
            TuiScreen.Tags => 3,
            _ => 5
        };

    private static ItemDisplayRow[] BuildForgottenRows(TuiSnapshot snapshot)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return snapshot.AllItems
            .Where(item => item.Type == ItemType.Task
                && item.Status == ItemStatus.Open
                && IsFromPreviousMonth(item.PublicRef, today))
            .OrderBy(item => item.PublicRef, StringComparer.Ordinal)
            .Select(ItemDisplayRow.From)
            .ToArray();
    }

    private static string FormatForgottenRow(ItemDisplayRow row)
    {
        return $"{row.Symbol} {row.PublicRef} {row.Content} previous month".Trim();
    }

    private static bool IsFromPreviousMonth(string publicRef, DateOnly today)
    {
        var parts = publicRef.Split('-');
        if (parts.Length < 3 || parts[1].Length != 4)
        {
            return false;
        }

        if (!int.TryParse(parts[1][..2], out var month) || !int.TryParse(parts[1][2..], out var yearTwoDigits))
        {
            return false;
        }

        var year = 2000 + yearTwoDigits;
        return year < today.Year || (year == today.Year && month < today.Month);
    }

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

    private static string[] BuildContextLines(MainDashboardViewModel viewModel, int forgottenCount)
    {
        var lines = new List<string>
        {
            "collections",
            $"> today      {viewModel.DayItems.Count}",
            $"  week       {viewModel.WeekItems.Count}",
            $"  month      {viewModel.MonthItems.Count}",
            $"  backlog    {viewModel.BacklogItems.Count}",
            $"  forgotten  {forgottenCount}",
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
