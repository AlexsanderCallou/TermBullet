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

            var generateButton = new Button("Generate");
            generateButton.Clicked += GenerateGuidedDraft;

            var applyButton = new Button("Apply");
            applyButton.Clicked += () =>
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
            };

            var discardButton = new Button("Discard");
            discardButton.Clicked += () =>
            {
                currentDraft = null;
                conversationLines.Add("system> draft discarded.");
                RenderPlanning();
            };

            var actionButtons = new[] { generateButton, applyButton, discardButton };

            BuildGuidedPlanningWorkspace(
                contentHost,
                navigation,
                guidedForm,
                conversationLines.Count > 0 ? conversationLines : vm.ConversationLines,
                currentDraft,
                actionButtons,
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
            conversationLines.Add($"system> generating {guidedForm.DetailLevelLabel.ToLowerInvariant()}-detail plan for {projectTag}...");
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
                ref focusedPanelIndex,
                navigation,
                onBack,
                onQuit,
                RenderPlanning);
            args.Handled = handled;
        };
    }

    private static bool HandlePlanningKey(
        KeyEvent keyEvent,
        ref int focusedPanelIndex,
        Tui.Navigation.TuiNavigationState navigation,
        Action onBack,
        Action onQuit,
        Action render)
    {
        switch (keyEvent.Key)
        {
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
        }

        return false;
    }

    private static void BuildGuidedPlanningWorkspace(
        View host,
        Tui.Navigation.TuiNavigationState navigation,
        GuidedPlanningForm form,
        IReadOnlyList<string> conversationLines,
        GenerateAiPlanningDraftResult? currentDraft,
        Button[] actionButtons,
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

        var detailLabel = new Label("Detail level:")
        {
            X = 0,
            Y = 0,
            Width = 14
        };
        var detailList = new ListView(TuiScreenUtilities.SanitizeListItems(BuildDetailRows(form.DetailLevel)))
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill(),
            Height = 2
        };
        detailList.SelectedItem = form.DetailLevel == GuidedPlanningDetailLevel.Low ? 1 : 0;
        var startTodayList = new ListView(TuiScreenUtilities.SanitizeListItems(BuildStartTodayRows(form.StartToday)))
        {
            X = 0,
            Y = 4,
            Width = Dim.Fill(),
            Height = 1
        };
        startTodayList.SelectedItem = 0;
        var syncingDetailSelection = false;

        detailList.SelectedItemChanged += _ =>
        {
            if (syncingDetailSelection)
            {
                return;
            }

            form.DetailLevel = detailList.SelectedItem == 1
                ? GuidedPlanningDetailLevel.Low
                : GuidedPlanningDetailLevel.High;
            syncingDetailSelection = true;
            try
            {
                TuiScreenUtilities.RefreshListView(detailList, BuildDetailRows(form.DetailLevel));
                detailList.SelectedItem = form.DetailLevel == GuidedPlanningDetailLevel.Low ? 1 : 0;
            }
            finally
            {
                syncingDetailSelection = false;
            }
        };
        startTodayList.KeyPress += args =>
        {
            if (args.KeyEvent.Key != Key.Space && args.KeyEvent.Key != Key.Enter)
            {
                return;
            }

            form.StartToday = !form.StartToday;
            TuiScreenUtilities.RefreshListView(startTodayList, BuildStartTodayRows(form.StartToday));
            startTodayList.SelectedItem = 0;
            args.Handled = true;
        };

        var rulesList = BuildList(BuildGuidedRuleLines(form));
        rulesList.Y = 6;
        rulesList.Height = Dim.Fill();
        rulesPanel.Add(detailLabel, detailList, startTodayList, rulesList);

        var previewPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(3, "Draft Preview", navigation, 2))
        {
            X = 0,
            Y = Pos.Bottom(setupPanel),
            Width = Dim.Fill(),
            Height = Dim.Fill(5)
        };
        var previewList = BuildList(BuildConversationLines(conversationLines, currentDraft));
        previewPanel.Add(previewList);

        var actionsPanel = new FrameView(TuiScreenUtilities.GetPanelTitle(4, "Actions", navigation, 3))
        {
            X = 0,
            Y = Pos.AnchorEnd(6),
            Width = Dim.Fill(),
            Height = 5
        };

        LayoutButtons(actionButtons, actionsPanel, maxPerLine: 4);

        host.Add(setupPanel, rulesPanel, previewPanel, actionsPanel);
        TuiScreenUtilities.UpdatePanelTitles(
            [setupPanel, rulesPanel, previewPanel, actionsPanel],
            ["Setup", "Rules", "Draft Preview", "Actions"],
            navigation);
        TuiScreenUtilities.FocusCurrentPanel([topicField, detailList, previewList, actionButtons[0]], navigation);
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

    private static void LayoutButtons(Button[] buttons, View parent, int maxPerLine)
    {
        var row = 0;
        var col = 0;
        Button? lastInRow = null;

        foreach (var button in buttons)
        {
            button.X = col == 0 ? 0 : Pos.Right(lastInRow!) + 2;
            button.Y = row;
            parent.Add(button);
            lastInRow = button;
            col++;

            if (col >= maxPerLine)
            {
                col = 0;
                row++;
                lastInRow = null;
            }
        }
    }

    private static IReadOnlyList<string> BuildConversationLines(
        IReadOnlyList<string> lines,
        GenerateAiPlanningDraftResult? currentDraft) =>
        PlanningConversationFormatter.Format(lines, currentDraft);

    private static IReadOnlyList<string> BuildGuidedRuleLines(GuidedPlanningForm form)
    {
        return
        [
            "High: each task = one atomic action.",
            "Low: each task = ~1 day or ~2h of work.",
            "",
            "Collection guardrails:",
            "  today: max 2",
            "  week: 2 to 10",
            "  month: 10+ (no limit)",
            "  backlog: remaining",
            "",
            "AI decides total task count.",
            "TermBullet controls tag and placement."
        ];
    }

    private static string[] BuildDetailRows(GuidedPlanningDetailLevel selectedDetailLevel) =>
    [
        TuiAsciiControls.RadioLine(selectedDetailLevel == GuidedPlanningDetailLevel.High, "High"),
        TuiAsciiControls.RadioLine(selectedDetailLevel == GuidedPlanningDetailLevel.Low, "Low")
    ];

    private static string[] BuildStartTodayRows(bool startToday) =>
    [
        TuiAsciiControls.CheckboxLine(startToday, "Start today")
    ];

    private static string BuildGuidedPlanningPrompt(GuidedPlanningForm form)
    {
        var detailInstruction = form.DetailLevel == GuidedPlanningDetailLevel.High
            ? "Each task must be a single atomic action (e.g., \"Install Rust with rustup\", \"Run cargo init\")."
            : "Each task must represent approximately 1 day or 2 hours of work. Group related atomic actions into meaningful tasks (e.g., \"Setup Rust development environment\").";

        var startTodayInstruction = form.StartToday
            ? "- Put at most 2 tasks in today."
            : "- Put 0 tasks in today.";

        return string.Join(Environment.NewLine, [
            "Create a new project planning draft from these guided inputs.",
            $"Topic: {form.Topic}",
            $"Project tag: {form.ProjectTag}",
            $"Detail level: {form.DetailLevelLabel}",
            $"Start today: {(form.StartToday ? "yes" : "no")}",
            "",
            "Hard constraints:",
            "- Return a structured draft, not chat.",
            "- Create only task actions.",
            $"- Every task must use tag \"{form.ProjectTag}\".",
            "- Every task content must start with a strictly increasing numeric prefix: 1. 2. 3. 4.",
            "- The prefix order must describe the execution order.",
            detailInstruction,
            "- You decide the total number of tasks based on the topic complexity.",
            startTodayInstruction,
            "- Put 2 to 10 tasks in week.",
            "- Put 10 or more tasks in month (no upper limit).",
            "- Put any remaining tasks in backlog.",
            "- Do not create notes or events.",
            "- Do not ask follow-up questions."
        ]);
    }
}

internal sealed class GuidedPlanningForm
{
    public string Topic { get; set; } = string.Empty;

    public string ProjectTag { get; set; } = string.Empty;

    public GuidedPlanningDetailLevel DetailLevel { get; set; } = GuidedPlanningDetailLevel.High;

    public bool StartToday { get; set; } = true;

    public string DetailLevelLabel => DetailLevel switch
    {
        GuidedPlanningDetailLevel.High => "High",
        GuidedPlanningDetailLevel.Low => "Low",
        _ => "High"
    };

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
}

internal enum GuidedPlanningDetailLevel
{
    High,
    Low
}
