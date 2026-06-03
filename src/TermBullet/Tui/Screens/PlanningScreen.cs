using Terminal.Gui;
using TermBullet.Application.Ai;
using TGui = Terminal.Gui.Application;

namespace TermBullet.Tui.Screens;

public static class PlanningScreen
{
    public static void Build(
        View root,
        Func<BuildAiPlanningRequest, CancellationToken, Task<GenerateAiPlanningResponseResult>>? generateAiPlanningResponse,
        Func<AiPlanningDraft, CancellationToken, Task<AiPlanningDraftApplyResult>>? applyAiPlanningDraft,
        Action onBack,
        Action onQuit,
        Action onRefresh,
        CancellationToken cancellationToken)
    {
        var mode = PlanningScreenMode.Hub;
        var navigation = new Tui.Navigation.TuiNavigationState(panelCount: 2);
        var selectedHubIndex = 0;
        var focusedPanelIndex = 0;
        var conversationLines = new List<string>();
        var conversationHistory = new List<AiPlanningMessage>();
        GenerateAiPlanningDraftResult? currentDraft = null;
        string? statusLine = null;
        TextField? promptField = null;

        var topBar = new Label(" TermBullet - Planning")
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill()
        };

        var contentHost = new View
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill(1)
        };

        var footer = new Label("")
        {
            X = 0,
            Y = Pos.AnchorEnd(1),
            Width = Dim.Fill()
        };

        root.Add(topBar, contentHost, footer);

        void RenderPlanning()
        {
            contentHost.RemoveAll();
            promptField = null;
            var vm = mode switch
            {
                PlanningScreenMode.NewPlanning => PlanningViewModel.ForNewPlanning(),
                PlanningScreenMode.RevisePlanning => PlanningViewModel.ForRevisePlanning(),
                _ => PlanningViewModel.ForHub()
            };

            footer.Text = $" {vm.Footer}";

            if (statusLine is not null)
            {
                conversationLines.Add($"system> {statusLine}");
                statusLine = null;
            }

            if (vm.Mode == PlanningScreenMode.Hub)
            {
                navigation = new Tui.Navigation.TuiNavigationState(panelCount: 2);
                if (!navigation.FocusPanel(focusedPanelIndex + 1))
                {
                    focusedPanelIndex = 0;
                }

                BuildHub(contentHost, vm, navigation, selectedHubIndex);
            }
            else
            {
                navigation = new Tui.Navigation.TuiNavigationState(panelCount: 4);
                if (!navigation.FocusPanel(focusedPanelIndex + 1))
                {
                    focusedPanelIndex = 0;
                }

                BuildWorkspace(
                    contentHost,
                    vm,
                    navigation,
                    conversationLines.Count > 0 ? conversationLines : vm.ConversationLines,
                    currentDraft,
                    field => promptField = field,
                    prompt =>
                    {
                        if (generateAiPlanningResponse is null)
                        {
                            conversationLines.Add("system> AI planning is not available.");
                            RenderPlanning();
                            return;
                        }

                        var requestMode = mode == PlanningScreenMode.RevisePlanning
                            ? AiPlanningMode.ReviseWeekly
                            : AiPlanningMode.NewProject;
                        var requireStructuredDraft = AiPlanningDraftIntent.RequiresStructuredDraft(prompt);
                        conversationLines.Add($"you> {prompt}");
                        conversationHistory.Add(new AiPlanningMessage(AiPlanningMessageRole.User, prompt));
                        conversationLines.Add("assistant> working...");
                        RenderPlanning();

                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                var result = await generateAiPlanningResponse(new BuildAiPlanningRequest
                                {
                                    Mode = requestMode,
                                    UserPrompt = prompt,
                                    ConversationHistory = conversationHistory
                                        .Take(Math.Max(0, conversationHistory.Count - 1))
                                        .ToArray(),
                                    RequireStructuredDraft = requireStructuredDraft
                                }, cancellationToken);
                                TGui.MainLoop?.Invoke(() =>
                                {
                                    conversationLines.Remove("assistant> working...");
                                    if (result.Draft is not null)
                                    {
                                        currentDraft = new GenerateAiPlanningDraftResult(
                                            result.Draft,
                                            result.ProviderModel,
                                            result.ModelRequest);
                                        var draftMessage = $"draft ready: {result.Draft.Actions.Count} actions.";
                                        conversationLines.Add($"assistant> {draftMessage}");
                                        conversationHistory.Add(new AiPlanningMessage(
                                            AiPlanningMessageRole.Assistant,
                                            $"{draftMessage} {result.Draft.Summary}"));
                                    }
                                    else
                                    {
                                        conversationLines.Add($"assistant> {result.AssistantMessage}");
                                        if (!string.IsNullOrWhiteSpace(result.AssistantMessage))
                                        {
                                            conversationHistory.Add(new AiPlanningMessage(
                                                AiPlanningMessageRole.Assistant,
                                                result.AssistantMessage));
                                        }
                                    }

                                    RenderPlanning();
                                });
                            }
                            catch (Exception exception) when (exception is not OperationCanceledException)
                            {
                                TGui.MainLoop?.Invoke(() =>
                                {
                                    conversationLines.Remove("assistant> working...");
                                    conversationLines.Add($"error> {exception.Message}");
                                    RenderPlanning();
                                });
                            }
                        }, cancellationToken);
                    });
            }
        }

        RenderPlanning();

        root.KeyPress += args =>
        {
            if (promptField?.HasFocus == true && PlanningShortcutPolicy.IsPromptTextInput(args.KeyEvent))
            {
                return;
            }

            if (TuiScreenUtilities.IsHelpKey(args.KeyEvent))
            {
                TuiScreenUtilities.ShowContextHelp(Tui.Navigation.TuiScreen.Planning);
                args.Handled = true;
                return;
            }

            var handled = HandlePlanningKey(
                args.KeyEvent,
                ref mode,
                ref selectedHubIndex,
                ref focusedPanelIndex,
                navigation,
                onBack,
                onQuit,
                () =>
                {
                    if (currentDraft is null)
                    {
                        conversationLines.Add("system> no draft to apply.");
                        RenderPlanning();
                        return;
                    }

                    if (applyAiPlanningDraft is null)
                    {
                        conversationLines.Add("system> draft application is not available.");
                        RenderPlanning();
                        return;
                    }

                    conversationLines.Add("system> applying draft...");
                    RenderPlanning();
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var result = await applyAiPlanningDraft(currentDraft.Draft, cancellationToken);
                            TGui.MainLoop?.Invoke(() =>
                            {
                                conversationLines.Add($"system> applied {result.Actions.Count} actions.");
                                currentDraft = null;
                                onRefresh();
                                RenderPlanning();
                            });
                        }
                        catch (Exception exception) when (exception is not OperationCanceledException)
                        {
                            TGui.MainLoop?.Invoke(() =>
                            {
                                conversationLines.Add($"error> {exception.Message}");
                                RenderPlanning();
                            });
                        }
                    }, cancellationToken);
                },
                () =>
                {
                    currentDraft = null;
                    conversationHistory.Clear();
                    conversationLines.Add("system> draft discarded.");
                    RenderPlanning();
                },
                RenderPlanning);
            args.Handled = handled;
        };
    }

    private static bool HandlePlanningKey(
        KeyEvent keyEvent,
        ref PlanningScreenMode mode,
        ref int selectedHubIndex,
        ref int focusedPanelIndex,
        Tui.Navigation.TuiNavigationState navigation,
        Action onBack,
        Action onQuit,
        Action onApply,
        Action onDiscard,
        Action render)
    {
        switch (keyEvent.Key)
        {
            case Key.q:
                onQuit();
                return true;
            case Key.Esc when mode == PlanningScreenMode.Hub:
                onBack();
                return true;
            case Key.Esc:
                mode = PlanningScreenMode.Hub;
                focusedPanelIndex = 0;
                render();
                return true;
            case Key.Tab:
                navigation.MoveNextPanel();
                focusedPanelIndex = navigation.FocusedPanelIndex;
                render();
                return true;
            case Key.BackTab:
                navigation.MovePreviousPanel();
                focusedPanelIndex = navigation.FocusedPanelIndex;
                render();
                return true;
            case Key.CursorUp when mode == PlanningScreenMode.Hub:
                selectedHubIndex = Math.Max(0, selectedHubIndex - 1);
                render();
                return true;
            case Key.CursorDown when mode == PlanningScreenMode.Hub:
                selectedHubIndex = Math.Min(1, selectedHubIndex + 1);
                render();
                return true;
            case Key.Enter when mode == PlanningScreenMode.Hub:
                mode = selectedHubIndex == 0
                    ? PlanningScreenMode.NewPlanning
                    : PlanningScreenMode.RevisePlanning;
                focusedPanelIndex = 3;
                render();
                return true;
            case Key a when a == (Key)'a' && mode != PlanningScreenMode.Hub:
                onApply();
                return true;
            case Key d when d == (Key)'d' && mode != PlanningScreenMode.Hub:
                onDiscard();
                return true;
        }

        return false;
    }

    private static void BuildHub(
        View host,
        PlanningViewModel vm,
        Tui.Navigation.TuiNavigationState navigation,
        int selectedHubIndex)
    {
        var primaryLines = vm.PrimaryLines.ToArray();
        primaryLines[0] = selectedHubIndex == 0 ? "> New Planning" : "  New Planning";
        primaryLines[1] = selectedHubIndex == 1 ? "> Revise Planning" : "  Revise Planning";

        var modePanel = new FrameView(TuiScreenUtilities.GetPanelTitle(1, "Planning Mode", navigation, 0))
        {
            X = 0,
            Y = 0,
            Width = Dim.Percent(35),
            Height = Dim.Fill()
        };
        var modeList = new ListView(TuiScreenUtilities.SanitizeListItems(primaryLines))
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        modePanel.Add(modeList);

        var previewPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(2, "Preview", navigation, 1))
        {
            X = Pos.Right(modePanel),
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        var previewList = new ListView(TuiScreenUtilities.SanitizeListItems(vm.SecondaryLines))
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        previewPanel.Add(previewList);

        host.Add(modePanel, previewPanel);
        TuiScreenUtilities.UpdatePanelTitles([modePanel, previewPanel], ["Planning Mode", "Preview"], navigation);
        TuiScreenUtilities.FocusCurrentPanel([modeList, previewList], navigation);
    }

    private static void BuildWorkspace(
        View host,
        PlanningViewModel vm,
        Tui.Navigation.TuiNavigationState navigation,
        IReadOnlyList<string> conversationLines,
        GenerateAiPlanningDraftResult? currentDraft,
        Action<TextField> onPromptFieldCreated,
        Action<string> onPrompt)
    {
        var setupPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(1, vm.Mode == PlanningScreenMode.NewPlanning ? "Setup" : "Review Scope", navigation, 0))
        {
            X = 0,
            Y = 0,
            Width = Dim.Percent(50),
            Height = Dim.Percent(30)
        };
        var setupList = BuildList(vm.PrimaryLines);
        setupPanel.Add(setupList);

        var actionsPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(2, "Draft Actions", navigation, 1))
        {
            X = Pos.Right(setupPanel),
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Percent(30)
        };
        var actionsList = BuildList(BuildDraftActionLines(vm.SecondaryLines, currentDraft));
        actionsPanel.Add(actionsList);

        var conversationPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(3, "Conversation", navigation, 2))
        {
            X = 0,
            Y = Pos.Bottom(setupPanel),
            Width = Dim.Fill(),
            Height = Dim.Fill(4)
        };
        var conversationList = BuildList(BuildConversationLines(conversationLines, currentDraft));
        conversationPanel.Add(conversationList);

        var promptPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(4, "Prompt", navigation, 3))
        {
            X = 0,
            Y = Pos.AnchorEnd(5),
            Width = Dim.Fill(),
            Height = 4
        };
        var promptField = new TextField("")
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = 1
        };
        promptField.KeyPress += args =>
        {
            if (args.KeyEvent.Key != Key.Enter)
            {
                return;
            }

            var value = promptField.Text?.ToString() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(value))
            {
                promptField.Text = string.Empty;
                onPrompt(value.Trim());
            }

            args.Handled = true;
        };
        promptPanel.Add(promptField);
        onPromptFieldCreated(promptField);

        host.Add(setupPanel, actionsPanel, conversationPanel, promptPanel);
        TuiScreenUtilities.UpdatePanelTitles(
            [setupPanel, actionsPanel, conversationPanel, promptPanel],
            [vm.Mode == PlanningScreenMode.NewPlanning ? "Setup" : "Review Scope", "Draft Actions", "Conversation", "Prompt"],
            navigation);
        TuiScreenUtilities.FocusCurrentPanel([setupList, actionsList, conversationList, promptField], navigation);
    }

    private static ListView BuildList(IReadOnlyList<string> lines) =>
        new(TuiScreenUtilities.SanitizeListItems(lines))
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

    private static IReadOnlyList<string> BuildDraftActionLines(
        IReadOnlyList<string> baseLines,
        GenerateAiPlanningDraftResult? currentDraft)
    {
        if (currentDraft is null)
        {
            return baseLines;
        }

        return
        [
            "Apply plan",
            "Discard draft",
            $"draft: {currentDraft.Draft.Actions.Count} actions",
            "press a to apply",
            "press d to discard"
        ];
    }

    private static IReadOnlyList<string> BuildConversationLines(
        IReadOnlyList<string> lines,
        GenerateAiPlanningDraftResult? currentDraft) =>
        PlanningConversationFormatter.Format(lines, currentDraft);
}
