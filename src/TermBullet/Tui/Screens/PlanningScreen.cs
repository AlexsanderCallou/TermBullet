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
        var mode = PlanningScreenMode.NewPlanning;
        var navigation = new Tui.Navigation.TuiNavigationState(panelCount: 4);
        var focusedPanelIndex = 0;
        var conversationLines = new List<string>();
        GenerateAiPlanningDraftResult? currentDraft = null;
        string? statusLine = null;
        TextField? guidedTopicField = null;
        TextField? guidedTagField = null;
        var guidedForm = new GuidedPlanningForm();
        IReadOnlyList<TextField> textInputFields = [];

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
            guidedTopicField = null;
            guidedTagField = null;
            textInputFields = [];
            var vm = PlanningViewModel.ForNewPlanning();

            footer.Text = $" {vm.Footer}";

            if (statusLine is not null)
            {
                conversationLines.Add($"system> {statusLine}");
                statusLine = null;
            }

            navigation = new Tui.Navigation.TuiNavigationState(panelCount: 4);
            if (!navigation.FocusPanel(focusedPanelIndex + 1))
            {
                focusedPanelIndex = 0;
            }

            BuildGuidedPlanningWorkspace(
                contentHost,
                vm,
                navigation,
                guidedForm,
                conversationLines.Count > 0 ? conversationLines : vm.ConversationLines,
                currentDraft,
                (topicField, tagField) =>
                {
                    guidedTopicField = topicField;
                    guidedTagField = tagField;
                    textInputFields = [topicField, tagField];
                });
        }

        RenderPlanning();

        void SyncGuidedFormFromFields()
        {
            if (guidedTopicField is not null)
            {
                guidedForm.Topic = guidedTopicField.Text?.ToString()?.Trim() ?? string.Empty;
            }

            if (guidedTagField is not null)
            {
                guidedForm.ProjectTag = guidedTagField.Text?.ToString()?.Trim() ?? string.Empty;
            }
        }

        void GenerateGuidedDraft()
        {
            SyncGuidedFormFromFields();
            if (generateAiPlanningResponse is null)
            {
                conversationLines.Add("system> AI planning is not available.");
                RenderPlanning();
                return;
            }

            var validationError = guidedForm.GetValidationError();
            if (validationError is not null)
            {
                conversationLines.Add($"system> {validationError}");
                RenderPlanning();
                return;
            }

            var projectTag = guidedForm.ProjectTag;
            var prompt = BuildGuidedPlanningPrompt(guidedForm);
            conversationLines.Clear();
            currentDraft = null;
            conversationLines.Add($"system> generating {guidedForm.VolumeLabel.ToLowerInvariant()} plan for {projectTag}...");
            conversationLines.Add("assistant> working...");
            RenderPlanning();

            _ = Task.Run(async () =>
            {
                try
                {
                    var result = await generateAiPlanningResponse(new BuildAiPlanningRequest
                    {
                        Mode = AiPlanningMode.NewProject,
                        UserPrompt = prompt,
                        ConversationHistory = [],
                        RequireStructuredDraft = true
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
                            conversationLines.Add($"assistant> draft ready: {result.Draft.Actions.Count} actions.");
                        }
                        else
                        {
                            conversationLines.Add($"error> {result.AssistantMessage ?? "AI planning response did not include a structured draft."}");
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
        }

        root.KeyPress += args =>
        {
            if (textInputFields.Any(field => field.HasFocus)
                && PlanningShortcutPolicy.IsPromptTextInput(args.KeyEvent))
            {
                return;
            }

            if (TuiScreenUtilities.IsHelpKey(args.KeyEvent))
            {
                TuiScreenUtilities.ShowContextHelp(Tui.Navigation.TuiScreen.Planning);
                args.Handled = true;
                return;
            }

            SyncGuidedFormFromFields();
            var handled = HandlePlanningKey(
                args.KeyEvent,
                ref mode,
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
                    conversationLines.Add("system> draft discarded.");
                    RenderPlanning();
                },
                GenerateGuidedDraft,
                () =>
                {
                    if (mode != PlanningScreenMode.NewPlanning)
                    {
                        return;
                    }

                    guidedForm.CycleVolume();
                    RenderPlanning();
                },
                () =>
                {
                    if (mode != PlanningScreenMode.NewPlanning)
                    {
                        return;
                    }

                    guidedForm.StartToday = !guidedForm.StartToday;
                    RenderPlanning();
                },
                RenderPlanning);
            args.Handled = handled;
        };
    }

    private static bool HandlePlanningKey(
        KeyEvent keyEvent,
        ref PlanningScreenMode mode,
        ref int focusedPanelIndex,
        Tui.Navigation.TuiNavigationState navigation,
        Action onBack,
        Action onQuit,
        Action onApply,
        Action onDiscard,
        Action onGenerateGuidedDraft,
        Action onCycleGuidedVolume,
        Action onToggleGuidedToday,
        Action render)
    {
        switch (keyEvent.Key)
        {
            case Key.q:
                onQuit();
                return true;
            case Key.Esc:
                onBack();
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
            case Key a when a == (Key)'a':
                onApply();
                return true;
            case Key d when d == (Key)'d':
                onDiscard();
                return true;
            case Key g when g == (Key)'g' && mode == PlanningScreenMode.NewPlanning:
                onGenerateGuidedDraft();
                return true;
            case Key s when s == (Key)'s' && mode == PlanningScreenMode.NewPlanning:
                onCycleGuidedVolume();
                return true;
            case Key t when t == (Key)'t' && mode == PlanningScreenMode.NewPlanning:
                onToggleGuidedToday();
                return true;
        }

        return false;
    }

    private static void BuildGuidedPlanningWorkspace(
        View host,
        PlanningViewModel vm,
        Tui.Navigation.TuiNavigationState navigation,
        GuidedPlanningForm form,
        IReadOnlyList<string> conversationLines,
        GenerateAiPlanningDraftResult? currentDraft,
        Action<TextField, TextField> onFieldsCreated)
    {
        var setupPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(1, "Setup", navigation, 0))
        {
            X = 0,
            Y = 0,
            Width = Dim.Percent(50),
            Height = Dim.Percent(42)
        };

        var topicLabel = new Label("Topic")
        {
            X = 0,
            Y = 0,
            Width = 12
        };
        var topicField = new TextField(form.Topic)
        {
            X = 13,
            Y = 0,
            Width = Dim.Fill(),
            Height = 1
        };
        var tagLabel = new Label("Project tag")
        {
            X = 0,
            Y = 2,
            Width = 12
        };
        var tagField = new TextField(form.ProjectTag)
        {
            X = 13,
            Y = 2,
            Width = Dim.Fill(),
            Height = 1
        };
        var setupHelp = BuildList([
            "s: cycle task volume",
            "t: toggle first task today",
            "g: generate structured draft",
            "All task titles must start with 1., 2., 3. in order."
        ]);
        setupHelp.X = 0;
        setupHelp.Y = 4;
        setupHelp.Width = Dim.Fill();
        setupHelp.Height = Dim.Fill();
        setupPanel.Add(topicLabel, topicField, tagLabel, tagField, setupHelp);

        var rulesPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(2, "Rules", navigation, 1))
        {
            X = Pos.Right(setupPanel),
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Percent(42)
        };
        var rulesList = BuildList(BuildGuidedRuleLines(form));
        rulesPanel.Add(rulesList);

        var previewPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(3, "Draft Preview", navigation, 2))
        {
            X = 0,
            Y = Pos.Bottom(setupPanel),
            Width = Dim.Fill(),
            Height = Dim.Fill(4)
        };
        var previewList = BuildList(BuildConversationLines(conversationLines, currentDraft));
        previewPanel.Add(previewList);

        var actionsPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(4, "Actions", navigation, 3))
        {
            X = 0,
            Y = Pos.AnchorEnd(5),
            Width = Dim.Fill(),
            Height = 4
        };
        var actionsList = BuildList(BuildDraftActionLines(vm.SecondaryLines, currentDraft));
        actionsPanel.Add(actionsList);

        host.Add(setupPanel, rulesPanel, previewPanel, actionsPanel);
        TuiScreenUtilities.UpdatePanelTitles(
            [setupPanel, rulesPanel, previewPanel, actionsPanel],
            ["Setup", "Rules", "Draft Preview", "Actions"],
            navigation);
        TuiScreenUtilities.FocusCurrentPanel([topicField, rulesList, previewList, actionsList], navigation);
        onFieldsCreated(topicField, tagField);
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

    private static IReadOnlyList<string> BuildGuidedRuleLines(GuidedPlanningForm form)
    {
        var (todayCount, weekCount, monthCount, backlogCount) = form.EstimateDistribution();

        return
        [
            $"Volume: {form.VolumeLabel}",
            $"Target range: {form.TargetRangeLabel}",
            $"Target tasks: {form.TargetTaskCount}",
            $"Start today: {(form.StartToday ? "Yes" : "No")}",
            "",
            $"Today: {todayCount}",
            $"Week: max 5 ({weekCount})",
            $"Month: max 20 ({monthCount})",
            $"Backlog: remaining ({backlogCount})",
            "",
            "TermBullet controls count, tag, and collections.",
            "The model only writes ordered task content."
        ];
    }

    private static string BuildGuidedPlanningPrompt(GuidedPlanningForm form)
    {
        var (todayCount, weekCount, monthCount, backlogCount) = form.EstimateDistribution();

        return string.Join(Environment.NewLine, [
            "Create a new project planning draft from these fixed guided inputs.",
            $"Topic: {form.Topic}",
            $"Project tag: {form.ProjectTag}",
            $"Task volume: {form.VolumeLabel}",
            $"Task count target: {form.TargetTaskCount}",
            $"Allowed task count range: {form.TargetRangeLabel}",
            $"Start today: {(form.StartToday ? "yes" : "no")}",
            "",
            "Hard constraints:",
            "- Return a structured draft, not chat.",
            "- Create only task actions.",
            $"- Every task must use tag \"{form.ProjectTag}\".",
            "- Every task content must start with a strictly increasing numeric prefix: 1. 2. 3. 4.",
            "- The prefix order must describe the execution order.",
            $"- Put {todayCount} task(s) in today.",
            $"- Put {weekCount} task(s) in week, never more than 5.",
            $"- Put {monthCount} task(s) in month, never more than 20.",
            $"- Put {backlogCount} task(s) in backlog.",
            "- Do not create notes or events.",
            "- Do not ask follow-up questions."
        ]);
    }
}

internal sealed class GuidedPlanningForm
{
    private static readonly GuidedPlanningVolume[] VolumeOrder =
    [
        GuidedPlanningVolume.Small,
        GuidedPlanningVolume.Medium,
        GuidedPlanningVolume.Large
    ];

    public string Topic { get; set; } = string.Empty;

    public string ProjectTag { get; set; } = string.Empty;

    public GuidedPlanningVolume Volume { get; private set; } = GuidedPlanningVolume.Medium;

    public bool StartToday { get; set; } = true;

    public string VolumeLabel => Volume switch
    {
        GuidedPlanningVolume.Small => "Small",
        GuidedPlanningVolume.Medium => "Medium",
        GuidedPlanningVolume.Large => "Large",
        _ => "Medium"
    };

    public string TargetRangeLabel => Volume switch
    {
        GuidedPlanningVolume.Small => "up to 10 tasks",
        GuidedPlanningVolume.Medium => "10-20 tasks",
        GuidedPlanningVolume.Large => "20-40 tasks",
        _ => "10-20 tasks"
    };

    public int TargetTaskCount => Volume switch
    {
        GuidedPlanningVolume.Small => 8,
        GuidedPlanningVolume.Medium => 15,
        GuidedPlanningVolume.Large => 30,
        _ => 15
    };

    public void CycleVolume()
    {
        var nextIndex = (Array.IndexOf(VolumeOrder, Volume) + 1) % VolumeOrder.Length;
        Volume = VolumeOrder[nextIndex];
    }

    public string? GetValidationError()
    {
        if (string.IsNullOrWhiteSpace(Topic))
        {
            return "Topic is required.";
        }

        if (string.IsNullOrWhiteSpace(ProjectTag))
        {
            return "Project tag is required.";
        }

        return null;
    }

    public (int Today, int Week, int Month, int Backlog) EstimateDistribution()
    {
        var remaining = TargetTaskCount;
        var today = StartToday && remaining > 0 ? 1 : 0;
        remaining -= today;

        var week = Math.Min(remaining, 5);
        remaining -= week;

        var month = Math.Min(remaining, 20);
        remaining -= month;

        return (today, week, month, remaining);
    }
}

internal enum GuidedPlanningVolume
{
    Small,
    Medium,
    Large
}
