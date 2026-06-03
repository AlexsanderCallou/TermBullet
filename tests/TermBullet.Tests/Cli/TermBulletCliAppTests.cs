using TermBullet.Services.Clock;
using TermBullet.Services.History;
using System.Text;
using TermBullet.Application.Ai;
using TermBullet.Application.History;
using TermBullet.Cli;
using TermBullet.Services.Ai;
using TermBullet.Services.Configuration;
<<<<<<< HEAD

namespace TermBullet.Tests.Cli;

=======

namespace TermBullet.Tests.Cli;

>>>>>>> 31d6ba16bacfc3554d22ce88aea847e70d502125
public sealed class TermBulletCliAppTests : IDisposable
{
    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        "termbullet-cli-ai-tests",
        Guid.NewGuid().ToString("N"));
<<<<<<< HEAD

    [Fact]
    public async Task InvokeAsync_runs_history_clear_for_specific_month()
    {
        var dependencies = CreateDependencies();
        var app = CreateApp(dependencies);
=======
>>>>>>> 31d6ba16bacfc3554d22ce88aea847e70d502125

    [Fact]
    public async Task InvokeAsync_runs_history_clear_for_specific_month()
    {
        var dependencies = CreateDependencies();
        var app = CreateApp(dependencies);

        var exitCode = await app.InvokeAsync(["history", "clear", "--month", "04_2026", "--force"]);

        Assert.Equal(0, exitCode);
        Assert.Equal((4, 2026), dependencies.HistoryService.ClearedMonth);
    }

    [Fact]
    public async Task InvokeAsync_runs_history_clear_for_all_months()
    {
        var dependencies = CreateDependencies();
        var app = CreateApp(dependencies);

        var exitCode = await app.InvokeAsync(["history", "clear", "--all", "--force"]);

        Assert.Equal(0, exitCode);
        Assert.True(dependencies.HistoryService.ClearAllCalled);
    }

    [Fact]
    public async Task InvokeAsync_runs_startup_action_before_command_dispatch()
    {
        var dependencies = CreateDependencies();
        var startupCalled = false;
        var app = CreateApp(dependencies, startupAction: _ =>
        {
            startupCalled = true;
            return Task.CompletedTask;
        });

        var exitCode = await app.InvokeAsync(["history", "clear", "--month", "04_2026", "--force"]);

        Assert.Equal(0, exitCode);
        Assert.True(startupCalled);
    }

    [Fact]
    public async Task InvokeAsync_writes_root_help_to_output()
    {
        var dependencies = CreateDependencies();
        var app = CreateApp(dependencies);

        var exitCode = await app.InvokeAsync(["--help"]);

        Assert.Equal(0, exitCode);
        Assert.Contains("TermBullet - Local-First Terminal Planner", dependencies.Output.ToString());
    }
<<<<<<< HEAD

    [Fact]
    public async Task InvokeAsync_writes_nested_help_to_output()
    {
        var dependencies = CreateDependencies();
        var app = CreateApp(dependencies);

        var exitCode = await app.InvokeAsync(["history", "clear", "--help"]);

        Assert.Equal(0, exitCode);
        Assert.Contains("Clear stored history entries", dependencies.Output.ToString());
        Assert.Contains("--month", dependencies.Output.ToString());
        Assert.Contains("--all", dependencies.Output.ToString());
    }

    [Fact]
    public async Task InvokeAsync_writes_version_for_version_flag()
    {
        var dependencies = CreateDependencies();
        var app = CreateApp(dependencies);

        var exitCode = await app.InvokeAsync(["--version"]);

        Assert.Equal(0, exitCode);
        Assert.Contains("1.3.0", dependencies.Output.ToString());
    }

    [Fact]
    public async Task InvokeAsync_writes_version_for_short_version_flag()
    {
        var dependencies = CreateDependencies();
        var app = CreateApp(dependencies);

        var exitCode = await app.InvokeAsync(["-v"]);

        Assert.Equal(0, exitCode);
        Assert.Contains("1.3.0", dependencies.Output.ToString());
    }

    [Fact]
=======

    [Fact]
    public async Task InvokeAsync_writes_nested_help_to_output()
    {
        var dependencies = CreateDependencies();
        var app = CreateApp(dependencies);

        var exitCode = await app.InvokeAsync(["history", "clear", "--help"]);

        Assert.Equal(0, exitCode);
        Assert.Contains("Clear stored history entries", dependencies.Output.ToString());
        Assert.Contains("--month", dependencies.Output.ToString());
        Assert.Contains("--all", dependencies.Output.ToString());
    }

    [Fact]
    public async Task InvokeAsync_writes_version_for_version_flag()
    {
        var dependencies = CreateDependencies();
        var app = CreateApp(dependencies);

        var exitCode = await app.InvokeAsync(["--version"]);

        Assert.Equal(0, exitCode);
        Assert.Contains("1.3.0", dependencies.Output.ToString());
    }

    [Fact]
    public async Task InvokeAsync_writes_version_for_short_version_flag()
    {
        var dependencies = CreateDependencies();
        var app = CreateApp(dependencies);

        var exitCode = await app.InvokeAsync(["-v"]);

        Assert.Equal(0, exitCode);
        Assert.Contains("1.3.0", dependencies.Output.ToString());
    }

    [Fact]
>>>>>>> 31d6ba16bacfc3554d22ce88aea847e70d502125
    public async Task InvokeAsync_runs_path_command_when_runtime_paths_are_available()
    {
        var dependencies = CreateDependencies();
        var runtimePaths = new TermBulletRuntimePaths(
            @"C:\TermBullet\conf.json",
            @"C:\TermBulletData",
            @"C:\TermBulletData\data");
        var app = CreateApp(dependencies, runtimePaths: runtimePaths);

        var exitCode = await app.InvokeAsync(["path"]);

        Assert.Equal(0, exitCode);
        var output = dependencies.Output.ToString();
        Assert.Contains("config: C:\\TermBullet\\conf.json", output);
        Assert.Contains("data_root: C:\\TermBulletData", output);
        Assert.Contains("data: C:\\TermBulletData\\data", output);
    }

    [Fact]
<<<<<<< HEAD
    public async Task InvokeAsync_test_ai_creates_template_when_missing()
    {
        var dependencies = CreateDependencies();
        var runtimePaths = CreateRuntimePaths();
        var app = CreateApp(dependencies, runtimePaths: runtimePaths, startupAction: _ => Task.CompletedTask);

        var exitCode = await app.InvokeAsync(["test-ai"]);

        Assert.Equal(1, exitCode);
        Assert.True(File.Exists(Path.Combine(runtimePaths.DataRoot, ".aiconf")));
        Assert.Contains("AI configuration file was created", dependencies.Error.ToString());
    }

    [Fact]
    public async Task InvokeAsync_set_ai_updates_default_profile_in_aiconf()
    {
        var dependencies = CreateDependencies();
        var runtimePaths = CreateRuntimePaths();
        Directory.CreateDirectory(runtimePaths.DataRoot);
        await File.WriteAllTextAsync(
            Path.Combine(runtimePaths.DataRoot, ".aiconf"),
            """
            [local-gemma]
            provider=openai-compatible
            model=gemma3:4b
            base_url=http://localhost:11434/v1
            api_key=ollama
            default=true

            [local-llama-fast]
            provider=openai-compatible
            model=llama3.2:1b
            base_url=http://localhost:11434/v1
            api_key=ollama
            """);
        var app = CreateApp(dependencies, runtimePaths: runtimePaths, startupAction: _ => Task.CompletedTask);

        var exitCode = await app.InvokeAsync(["set-ai", "local-llama-fast"]);

        Assert.Equal(0, exitCode);
        var config = await new AiConfigurationFileService(runtimePaths.DataRoot).LoadConfigAsync();
        Assert.Equal("local-llama-fast", config.Ai?.ActiveProfile);
        Assert.Contains("active AI profile: local-llama-fast", dependencies.Output.ToString());
    }

    [Fact]
    public async Task InvokeAsync_test_ai_uses_active_aiconf_profile()
    {
        var dependencies = CreateDependencies();
        var runtimePaths = CreateRuntimePaths();
        Directory.CreateDirectory(runtimePaths.DataRoot);
        await File.WriteAllTextAsync(
            Path.Combine(runtimePaths.DataRoot, ".aiconf"),
            """
            [local-gemma]
            provider=openai-compatible
            model=gemma3:4b
            base_url=http://localhost:11434/v1
            api_key=ollama
            default=true
            timeout_seconds=180
            """);
        var testedProfile = string.Empty;
        var app = CreateApp(
            dependencies,
            runtimePaths: runtimePaths,
            startupAction: _ => Task.CompletedTask,
            testAiProfileConnection: (profileName, _) =>
            {
                testedProfile = profileName ?? string.Empty;
                return Task.FromResult(new AiPlanningProviderResponse("ok", "gemma3:4b"));
            });

        var exitCode = await app.InvokeAsync(["test-ai"]);

        Assert.Equal(0, exitCode);
        Assert.Equal("local-gemma", testedProfile);
        var output = dependencies.Output.ToString();
        Assert.Contains("profile valid: local-gemma", output);
        Assert.Contains("provider reachable: gemma3:4b", output);
    }

    [Fact]
    public async Task InvokeAsync_adds_ai_profile_to_config()
    {
        var dependencies = CreateDependencies();
        var runtimePaths = CreateRuntimePaths();
        var app = CreateApp(dependencies, runtimePaths: runtimePaths, startupAction: _ => Task.CompletedTask);

        var exitCode = await app.InvokeAsync([
            "ai", "profile", "add", "gpt",
            "--provider", "openai",
            "--model", "gpt-4.1-mini",
            "--base-url", "https://api.openai.com/v1",
            "--api-key-env", "TERMBULLET_OPENAI_API_KEY"
        ]);

        Assert.Equal(0, exitCode);
        Assert.Contains("profile saved: gpt", dependencies.Output.ToString());
        var config = await new TermBulletConfigService(_root).LoadAsync();
        Assert.NotNull(config);
        Assert.Equal("gpt", config.Ai?.ActiveProfile);
        Assert.Equal("openai", config.Ai?.Profiles["gpt"].Provider);
        Assert.Equal("TERMBULLET_OPENAI_API_KEY", config.Ai?.Profiles["gpt"].ApiKeyEnv);
    }

    [Fact]
    public async Task InvokeAsync_lists_ai_profiles_without_secrets()
    {
        var dependencies = CreateDependencies();
        var runtimePaths = CreateRuntimePaths();
        await new TermBulletConfigService(_root).SaveAsync(new TermBulletConfig(
            runtimePaths.DataRoot,
            new AiConfiguration(
                "gpt",
                new Dictionary<string, AiProfile>
                {
                    ["gpt"] = new("openai", "gpt-4.1-mini", ApiKeyEnv: "TERMBULLET_OPENAI_API_KEY"),
                    ["local"] = new("openai-compatible", "llama3.1", "http://localhost:11434/v1", "none")
                })));
        var app = CreateApp(dependencies, runtimePaths: runtimePaths, startupAction: _ => Task.CompletedTask);

        var exitCode = await app.InvokeAsync(["ai", "profile", "list"]);

        Assert.Equal(0, exitCode);
        var output = dependencies.Output.ToString();
        Assert.Contains("* gpt", output);
        Assert.Contains("local", output);
        Assert.Contains("gpt-4.1-mini", output);
        Assert.DoesNotContain("TERMBULLET_OPENAI_API_KEY", output);
    }

    [Fact]
    public async Task InvokeAsync_selects_active_ai_profile()
    {
        var dependencies = CreateDependencies();
        var runtimePaths = CreateRuntimePaths();
        await new TermBulletConfigService(_root).SaveAsync(new TermBulletConfig(
            runtimePaths.DataRoot,
            new AiConfiguration(
                "gpt",
                new Dictionary<string, AiProfile>
                {
                    ["gpt"] = new("openai", "gpt-4.1-mini"),
                    ["local"] = new("openai-compatible", "llama3.1", ApiKeySource: "none")
                })));
        var app = CreateApp(dependencies, runtimePaths: runtimePaths, startupAction: _ => Task.CompletedTask);

        var exitCode = await app.InvokeAsync(["ai", "profile", "use", "local"]);

        Assert.Equal(0, exitCode);
        var config = await new TermBulletConfigService(_root).LoadAsync();
        Assert.Equal("local", config?.Ai?.ActiveProfile);
    }

    [Fact]
    public async Task InvokeAsync_shows_ai_profile_without_secret_value()
    {
        var dependencies = CreateDependencies();
        var runtimePaths = CreateRuntimePaths();
        await new TermBulletConfigService(_root).SaveAsync(new TermBulletConfig(
            runtimePaths.DataRoot,
            new AiConfiguration(
                "gpt",
                new Dictionary<string, AiProfile>
                {
                    ["gpt"] = new("openai", "gpt-4.1-mini", ApiKeyEnv: "TERMBULLET_OPENAI_API_KEY")
                })));
        var app = CreateApp(dependencies, runtimePaths: runtimePaths, startupAction: _ => Task.CompletedTask);

        var exitCode = await app.InvokeAsync(["ai", "profile", "show", "gpt"]);

        Assert.Equal(0, exitCode);
        var output = dependencies.Output.ToString();
        Assert.Contains("profile: gpt", output);
        Assert.Contains("provider: openai", output);
        Assert.Contains("api_key_env: TERMBULLET_OPENAI_API_KEY", output);
        Assert.DoesNotContain("api_key:", output);
    }

    [Fact]
    public async Task InvokeAsync_removes_ai_profile_and_clears_active_profile()
    {
        var dependencies = CreateDependencies();
        var runtimePaths = CreateRuntimePaths();
        await new TermBulletConfigService(_root).SaveAsync(new TermBulletConfig(
            runtimePaths.DataRoot,
            new AiConfiguration(
                "gpt",
                new Dictionary<string, AiProfile>
                {
                    ["gpt"] = new("openai", "gpt-4.1-mini")
                })));
        var app = CreateApp(dependencies, runtimePaths: runtimePaths, startupAction: _ => Task.CompletedTask);

        var exitCode = await app.InvokeAsync(["ai", "profile", "remove", "gpt"]);

        Assert.Equal(0, exitCode);
        var config = await new TermBulletConfigService(_root).LoadAsync();
        Assert.Null(config?.Ai);
    }

    [Fact]
    public async Task InvokeAsync_tests_ai_profile_configuration_and_provider_connection()
    {
        var dependencies = CreateDependencies();
        var runtimePaths = CreateRuntimePaths();
        await new TermBulletConfigService(_root).SaveAsync(new TermBulletConfig(
            runtimePaths.DataRoot,
            new AiConfiguration(
                "local",
                new Dictionary<string, AiProfile>
                {
                    ["local"] = new("openai-compatible", "llama3.1", "http://localhost:11434/v1", "none")
                })));
        var testedProfile = string.Empty;
        var app = CreateApp(
            dependencies,
            runtimePaths: runtimePaths,
            startupAction: _ => Task.CompletedTask,
            testAiProfileConnection: (profileName, _) =>
            {
                testedProfile = profileName ?? string.Empty;
                return Task.FromResult(new AiPlanningProviderResponse("ok", "llama3.1"));
            });

        var exitCode = await app.InvokeAsync(["ai", "profile", "test", "local"]);

        Assert.Equal(0, exitCode);
        Assert.Equal("local", testedProfile);
        var output = dependencies.Output.ToString();
        Assert.Contains("profile valid: local", output);
        Assert.Contains("provider reachable: llama3.1", output);
    }

    [Fact]
    public async Task InvokeAsync_generates_ai_plan_preview()
    {
        var dependencies = CreateDependencies();
        BuildAiPlanningRequest? capturedRequest = null;
        var app = CreateApp(
            dependencies,
            runtimePaths: CreateRuntimePaths(),
            startupAction: _ => Task.CompletedTask,
            generateAiPlanningDraft: (request, _) =>
            {
                capturedRequest = request;
                return Task.FromResult(new GenerateAiPlanningDraftResult(
                    AiPlanningDraftParser.Parse(
                        """
                        {
                          "mode": "new_weekly",
                          "summary": "Weekly plan.",
                          "actions": [
                            {
                              "type": "create_task",
                              "tag": "default",
                              "collection": "week",
                              "content": "Review open tasks"
                            }
                          ]
                        }
                        """),
                    "test-model",
                    new AiPlanningModelRequest(AiPlanningMode.NewWeekly, null, [], [])));
            });

        var exitCode = await app.InvokeAsync(["ai", "plan", "new-weekly", "--prompt", "Plan my week."]);

        Assert.Equal(0, exitCode);
        Assert.NotNull(capturedRequest);
        Assert.Equal(AiPlanningMode.NewWeekly, capturedRequest.Mode);
        Assert.Equal("Plan my week.", capturedRequest.UserPrompt);
        var output = dependencies.Output.ToString();
        Assert.Contains("model: test-model", output);
        Assert.Contains("mode: new_weekly", output);
        Assert.Contains("summary: Weekly plan.", output);
        Assert.Contains("1. create_task", output);
        Assert.Contains("content: Review open tasks", output);
    }

    [Fact]
    public async Task InvokeAsync_cancels_ai_plan_apply_when_interactive_confirmation_is_rejected()
    {
        var dependencies = CreateDependencies();
        var applyCalled = false;
        var app = CreateApp(
            dependencies,
            runtimePaths: CreateRuntimePaths(),
            startupAction: _ => Task.CompletedTask,
            generateAiPlanningDraft: (_, _) => Task.FromResult(CreateWeeklyDraftResult()),
            applyAiPlanningDraft: (_, _) =>
            {
                applyCalled = true;
                return Task.FromResult(new AiPlanningDraftApplyResult([]));
            },
            input: new StringReader("no"));

        var exitCode = await app.InvokeAsync(["ai", "plan", "new-weekly", "--prompt", "Plan my week.", "--apply"]);

        Assert.Equal(0, exitCode);
        Assert.False(applyCalled);
        var output = dependencies.Output.ToString();
        Assert.Contains("confirm apply draft?", output);
        Assert.Contains("apply cancelled", output);
    }

    [Fact]
    public async Task InvokeAsync_applies_ai_plan_when_yes_flag_confirms()
    {
        var dependencies = CreateDependencies();
        AiPlanningDraft? appliedDraft = null;
        var app = CreateApp(
            dependencies,
            runtimePaths: CreateRuntimePaths(),
            startupAction: _ => Task.CompletedTask,
            generateAiPlanningDraft: (_, _) => Task.FromResult(CreateWeeklyDraftResult()),
            applyAiPlanningDraft: (draft, _) =>
            {
                appliedDraft = draft;
                return Task.FromResult(new AiPlanningDraftApplyResult(
                [
                    new AiPlanningDraftAppliedAction("create_task", "t-0626-1", "default", "week")
                ]));
            });

        var exitCode = await app.InvokeAsync(["ai", "plan", "new-weekly", "--prompt", "Plan my week.", "--apply", "--yes"]);

        Assert.Equal(0, exitCode);
        Assert.NotNull(appliedDraft);
        var output = dependencies.Output.ToString();
        Assert.Contains("applied:", output);
        Assert.Contains("1. create_task t-0626-1", output);
    }

    [Fact]
    public async Task InvokeAsync_applies_ai_plan_when_interactive_confirmation_is_accepted()
    {
        var dependencies = CreateDependencies();
        AiPlanningDraft? appliedDraft = null;
        var app = CreateApp(
            dependencies,
            runtimePaths: CreateRuntimePaths(),
            startupAction: _ => Task.CompletedTask,
            generateAiPlanningDraft: (_, _) => Task.FromResult(CreateWeeklyDraftResult()),
            applyAiPlanningDraft: (draft, _) =>
            {
                appliedDraft = draft;
                return Task.FromResult(new AiPlanningDraftApplyResult(
                [
                    new AiPlanningDraftAppliedAction("create_task", "t-0626-1", "default", "week")
                ]));
            },
            input: new StringReader("yes"));

        var exitCode = await app.InvokeAsync(["ai", "plan", "new-weekly", "--prompt", "Plan my week.", "--apply"]);

        Assert.Equal(0, exitCode);
        Assert.NotNull(appliedDraft);
        var output = dependencies.Output.ToString();
        Assert.Contains("confirm apply draft?", output);
        Assert.Contains("applied:", output);
    }

    [Fact]
    public async Task InvokeAsync_runs_ai_chat_and_generates_draft_preview()
    {
        var dependencies = CreateDependencies();
        BuildAiPlanningRequest? capturedRequest = null;
        var app = CreateApp(
            dependencies,
            runtimePaths: CreateRuntimePaths(),
            startupAction: _ => Task.CompletedTask,
            generateAiPlanningDraft: (request, _) =>
            {
                capturedRequest = request;
                return Task.FromResult(CreateWeeklyDraftResult());
            },
            input: new StringReader("Plan my week.\n/exit\n"));

        var exitCode = await app.InvokeAsync(["ai", "chat", "--mode", "new-weekly"]);

        Assert.Equal(0, exitCode);
        Assert.NotNull(capturedRequest);
        Assert.Equal(AiPlanningMode.NewWeekly, capturedRequest.Mode);
        Assert.Equal("Plan my week.", capturedRequest.UserPrompt);
        var output = dependencies.Output.ToString();
        Assert.Contains("mode: new-weekly", output);
        Assert.Contains("draft ready", output);
        Assert.Contains("summary: Weekly plan.", output);
    }

    [Fact]
    public async Task InvokeAsync_ai_chat_discards_current_draft()
    {
        var dependencies = CreateDependencies();
        var app = CreateApp(
            dependencies,
            runtimePaths: CreateRuntimePaths(),
            startupAction: _ => Task.CompletedTask,
            generateAiPlanningDraft: (_, _) => Task.FromResult(CreateWeeklyDraftResult()),
            input: new StringReader("Plan my week.\n/discard\n/exit\n"));

        var exitCode = await app.InvokeAsync(["ai", "chat", "--mode", "new-weekly"]);

        Assert.Equal(0, exitCode);
        Assert.Contains("draft discarded", dependencies.Output.ToString());
    }

    [Fact]
    public async Task InvokeAsync_ai_chat_sends_previous_turns_as_conversation_history()
    {
        var dependencies = CreateDependencies();
        var capturedRequests = new List<BuildAiPlanningRequest>();
        var app = CreateApp(
            dependencies,
            runtimePaths: CreateRuntimePaths(),
            startupAction: _ => Task.CompletedTask,
            generateAiPlanningResponse: (request, _) =>
            {
                capturedRequests.Add(request);
                if (capturedRequests.Count == 1)
                {
                    return Task.FromResult(new GenerateAiPlanningResponseResult(
                        Draft: null,
                        AssistantMessage: "Roadmap: ownership, borrowing, structs, enums, modules.",
                        ProviderModel: "test-model",
                        ModelRequest: new AiPlanningModelRequest(AiPlanningMode.NewProject, null, [], [])));
                }

                return Task.FromResult(new GenerateAiPlanningResponseResult(
                    CreateProjectDraft(),
                    AssistantMessage: null,
                    ProviderModel: "test-model",
                    ModelRequest: new AiPlanningModelRequest(AiPlanningMode.NewProject, null, [], [])));
            },
            input: new StringReader("Help me think about a Rust roadmap.\nCreate the tasks.\n/exit\n"));

        var exitCode = await app.InvokeAsync(["ai", "chat", "--mode", "new-project"]);

        Assert.Equal(0, exitCode);
        Assert.Equal(2, capturedRequests.Count);
        Assert.Empty(capturedRequests[0].ConversationHistory);
        Assert.False(capturedRequests[0].RequireStructuredDraft);
        Assert.Equal(2, capturedRequests[1].ConversationHistory.Count);
        Assert.True(capturedRequests[1].RequireStructuredDraft);
        Assert.Equal(AiPlanningMessageRole.User, capturedRequests[1].ConversationHistory[0].Role);
        Assert.Equal("Help me think about a Rust roadmap.", capturedRequests[1].ConversationHistory[0].Content);
        Assert.Equal(AiPlanningMessageRole.Assistant, capturedRequests[1].ConversationHistory[1].Role);
        Assert.Contains("ownership", capturedRequests[1].ConversationHistory[1].Content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task InvokeAsync_ai_chat_applies_current_draft_after_confirmation()
    {
        var dependencies = CreateDependencies();
        AiPlanningDraft? appliedDraft = null;
        var app = CreateApp(
            dependencies,
            runtimePaths: CreateRuntimePaths(),
            startupAction: _ => Task.CompletedTask,
            generateAiPlanningDraft: (_, _) => Task.FromResult(CreateWeeklyDraftResult()),
            applyAiPlanningDraft: (draft, _) =>
            {
                appliedDraft = draft;
                return Task.FromResult(new AiPlanningDraftApplyResult(
                [
                    new AiPlanningDraftAppliedAction("create_task", "t-0626-1", "default", "week")
                ]));
            },
            input: new StringReader("Plan my week.\n/apply\nyes\n/exit\n"));

        var exitCode = await app.InvokeAsync(["ai", "chat", "--mode", "new-weekly"]);

        Assert.Equal(0, exitCode);
        Assert.NotNull(appliedDraft);
        Assert.Contains("applied:", dependencies.Output.ToString());
    }

    [Fact]
    public async Task InvokeAsync_writes_parse_error_for_unknown_command()
=======
    public async Task InvokeAsync_adds_ai_profile_to_config()
>>>>>>> 31d6ba16bacfc3554d22ce88aea847e70d502125
    {
        var dependencies = CreateDependencies();
        var runtimePaths = CreateRuntimePaths();
        var app = CreateApp(dependencies, runtimePaths: runtimePaths, startupAction: _ => Task.CompletedTask);

        var exitCode = await app.InvokeAsync([
            "ai", "profile", "add", "gpt",
            "--provider", "openai",
            "--model", "gpt-4.1-mini",
            "--base-url", "https://api.openai.com/v1",
            "--api-key-env", "TERMBULLET_OPENAI_API_KEY"
        ]);

        Assert.Equal(0, exitCode);
        Assert.Contains("profile saved: gpt", dependencies.Output.ToString());
        var config = await new TermBulletConfigService(_root).LoadAsync();
        Assert.NotNull(config);
        Assert.Equal("gpt", config.Ai?.ActiveProfile);
        Assert.Equal("openai", config.Ai?.Profiles["gpt"].Provider);
        Assert.Equal("TERMBULLET_OPENAI_API_KEY", config.Ai?.Profiles["gpt"].ApiKeyEnv);
    }

    [Fact]
    public async Task InvokeAsync_lists_ai_profiles_without_secrets()
    {
        var dependencies = CreateDependencies();
        var runtimePaths = CreateRuntimePaths();
        await new TermBulletConfigService(_root).SaveAsync(new TermBulletConfig(
            runtimePaths.DataRoot,
            new AiConfiguration(
                "gpt",
                new Dictionary<string, AiProfile>
                {
                    ["gpt"] = new("openai", "gpt-4.1-mini", ApiKeyEnv: "TERMBULLET_OPENAI_API_KEY"),
                    ["local"] = new("openai-compatible", "llama3.1", "http://localhost:11434/v1", "none")
                })));
        var app = CreateApp(dependencies, runtimePaths: runtimePaths, startupAction: _ => Task.CompletedTask);

        var exitCode = await app.InvokeAsync(["ai", "profile", "list"]);

        Assert.Equal(0, exitCode);
        var output = dependencies.Output.ToString();
        Assert.Contains("* gpt", output);
        Assert.Contains("local", output);
        Assert.Contains("gpt-4.1-mini", output);
        Assert.DoesNotContain("TERMBULLET_OPENAI_API_KEY", output);
    }

    [Fact]
    public async Task InvokeAsync_selects_active_ai_profile()
    {
        var dependencies = CreateDependencies();
        var runtimePaths = CreateRuntimePaths();
        await new TermBulletConfigService(_root).SaveAsync(new TermBulletConfig(
            runtimePaths.DataRoot,
            new AiConfiguration(
                "gpt",
                new Dictionary<string, AiProfile>
                {
                    ["gpt"] = new("openai", "gpt-4.1-mini"),
                    ["local"] = new("openai-compatible", "llama3.1", ApiKeySource: "none")
                })));
        var app = CreateApp(dependencies, runtimePaths: runtimePaths, startupAction: _ => Task.CompletedTask);

        var exitCode = await app.InvokeAsync(["ai", "profile", "use", "local"]);

        Assert.Equal(0, exitCode);
        var config = await new TermBulletConfigService(_root).LoadAsync();
        Assert.Equal("local", config?.Ai?.ActiveProfile);
    }

    [Fact]
    public async Task InvokeAsync_shows_ai_profile_without_secret_value()
    {
        var dependencies = CreateDependencies();
        var runtimePaths = CreateRuntimePaths();
        await new TermBulletConfigService(_root).SaveAsync(new TermBulletConfig(
            runtimePaths.DataRoot,
            new AiConfiguration(
                "gpt",
                new Dictionary<string, AiProfile>
                {
                    ["gpt"] = new("openai", "gpt-4.1-mini", ApiKeyEnv: "TERMBULLET_OPENAI_API_KEY")
                })));
        var app = CreateApp(dependencies, runtimePaths: runtimePaths, startupAction: _ => Task.CompletedTask);

        var exitCode = await app.InvokeAsync(["ai", "profile", "show", "gpt"]);

        Assert.Equal(0, exitCode);
        var output = dependencies.Output.ToString();
        Assert.Contains("profile: gpt", output);
        Assert.Contains("provider: openai", output);
        Assert.Contains("api_key_env: TERMBULLET_OPENAI_API_KEY", output);
        Assert.DoesNotContain("api_key:", output);
    }

    [Fact]
    public async Task InvokeAsync_removes_ai_profile_and_clears_active_profile()
    {
        var dependencies = CreateDependencies();
        var runtimePaths = CreateRuntimePaths();
        await new TermBulletConfigService(_root).SaveAsync(new TermBulletConfig(
            runtimePaths.DataRoot,
            new AiConfiguration(
                "gpt",
                new Dictionary<string, AiProfile>
                {
                    ["gpt"] = new("openai", "gpt-4.1-mini")
                })));
        var app = CreateApp(dependencies, runtimePaths: runtimePaths, startupAction: _ => Task.CompletedTask);

        var exitCode = await app.InvokeAsync(["ai", "profile", "remove", "gpt"]);

        Assert.Equal(0, exitCode);
        var config = await new TermBulletConfigService(_root).LoadAsync();
        Assert.Null(config?.Ai);
    }

    [Fact]
    public async Task InvokeAsync_tests_ai_profile_configuration_and_provider_connection()
    {
        var dependencies = CreateDependencies();
        var runtimePaths = CreateRuntimePaths();
        await new TermBulletConfigService(_root).SaveAsync(new TermBulletConfig(
            runtimePaths.DataRoot,
            new AiConfiguration(
                "local",
                new Dictionary<string, AiProfile>
                {
                    ["local"] = new("openai-compatible", "llama3.1", "http://localhost:11434/v1", "none")
                })));
        var testedProfile = string.Empty;
        var app = CreateApp(
            dependencies,
            runtimePaths: runtimePaths,
            startupAction: _ => Task.CompletedTask,
            testAiProfileConnection: (_, profileName, _) =>
            {
                testedProfile = profileName;
                return Task.FromResult(new AiPlanningProviderResponse("ok", "llama3.1"));
            });

        var exitCode = await app.InvokeAsync(["ai", "profile", "test", "local"]);

        Assert.Equal(0, exitCode);
        Assert.Equal("local", testedProfile);
        var output = dependencies.Output.ToString();
        Assert.Contains("profile valid: local", output);
        Assert.Contains("provider reachable: llama3.1", output);
    }

    [Fact]
    public async Task InvokeAsync_generates_ai_plan_preview()
    {
        var dependencies = CreateDependencies();
        BuildAiPlanningRequest? capturedRequest = null;
        var app = CreateApp(
            dependencies,
            runtimePaths: CreateRuntimePaths(),
            startupAction: _ => Task.CompletedTask,
            generateAiPlanningDraft: (request, _) =>
            {
                capturedRequest = request;
                return Task.FromResult(new GenerateAiPlanningDraftResult(
                    AiPlanningDraftParser.Parse(
                        """
                        {
                          "mode": "new_weekly",
                          "summary": "Weekly plan.",
                          "actions": [
                            {
                              "type": "create_task",
                              "tag": "default",
                              "collection": "week",
                              "content": "Review open tasks"
                            }
                          ]
                        }
                        """),
                    "test-model",
                    new AiPlanningModelRequest(AiPlanningMode.NewWeekly, null, [], [])));
            });

        var exitCode = await app.InvokeAsync(["ai", "plan", "new-weekly", "--prompt", "Plan my week."]);

        Assert.Equal(0, exitCode);
        Assert.NotNull(capturedRequest);
        Assert.Equal(AiPlanningMode.NewWeekly, capturedRequest.Mode);
        Assert.Equal("Plan my week.", capturedRequest.UserPrompt);
        var output = dependencies.Output.ToString();
        Assert.Contains("model: test-model", output);
        Assert.Contains("mode: new_weekly", output);
        Assert.Contains("summary: Weekly plan.", output);
        Assert.Contains("1. create_task", output);
        Assert.Contains("content: Review open tasks", output);
    }

    [Fact]
    public async Task InvokeAsync_cancels_ai_plan_apply_when_interactive_confirmation_is_rejected()
    {
        var dependencies = CreateDependencies();
        var applyCalled = false;
        var app = CreateApp(
            dependencies,
            runtimePaths: CreateRuntimePaths(),
            startupAction: _ => Task.CompletedTask,
            generateAiPlanningDraft: (_, _) => Task.FromResult(CreateWeeklyDraftResult()),
            applyAiPlanningDraft: (_, _) =>
            {
                applyCalled = true;
                return Task.FromResult(new AiPlanningDraftApplyResult([]));
            },
            input: new StringReader("no"));

        var exitCode = await app.InvokeAsync(["ai", "plan", "new-weekly", "--prompt", "Plan my week.", "--apply"]);

        Assert.Equal(0, exitCode);
        Assert.False(applyCalled);
        var output = dependencies.Output.ToString();
        Assert.Contains("confirm apply draft?", output);
        Assert.Contains("apply cancelled", output);
    }

    [Fact]
    public async Task InvokeAsync_applies_ai_plan_when_yes_flag_confirms()
    {
        var dependencies = CreateDependencies();
        AiPlanningDraft? appliedDraft = null;
        var app = CreateApp(
            dependencies,
            runtimePaths: CreateRuntimePaths(),
            startupAction: _ => Task.CompletedTask,
            generateAiPlanningDraft: (_, _) => Task.FromResult(CreateWeeklyDraftResult()),
            applyAiPlanningDraft: (draft, _) =>
            {
                appliedDraft = draft;
                return Task.FromResult(new AiPlanningDraftApplyResult(
                [
                    new AiPlanningDraftAppliedAction("create_task", "t-0626-1", "default", "week")
                ]));
            });

        var exitCode = await app.InvokeAsync(["ai", "plan", "new-weekly", "--prompt", "Plan my week.", "--apply", "--yes"]);

        Assert.Equal(0, exitCode);
        Assert.NotNull(appliedDraft);
        var output = dependencies.Output.ToString();
        Assert.Contains("applied:", output);
        Assert.Contains("1. create_task t-0626-1", output);
    }

    [Fact]
    public async Task InvokeAsync_applies_ai_plan_when_interactive_confirmation_is_accepted()
    {
        var dependencies = CreateDependencies();
        AiPlanningDraft? appliedDraft = null;
        var app = CreateApp(
            dependencies,
            runtimePaths: CreateRuntimePaths(),
            startupAction: _ => Task.CompletedTask,
            generateAiPlanningDraft: (_, _) => Task.FromResult(CreateWeeklyDraftResult()),
            applyAiPlanningDraft: (draft, _) =>
            {
                appliedDraft = draft;
                return Task.FromResult(new AiPlanningDraftApplyResult(
                [
                    new AiPlanningDraftAppliedAction("create_task", "t-0626-1", "default", "week")
                ]));
            },
            input: new StringReader("yes"));

        var exitCode = await app.InvokeAsync(["ai", "plan", "new-weekly", "--prompt", "Plan my week.", "--apply"]);

        Assert.Equal(0, exitCode);
        Assert.NotNull(appliedDraft);
        var output = dependencies.Output.ToString();
        Assert.Contains("confirm apply draft?", output);
        Assert.Contains("applied:", output);
    }

    [Fact]
    public async Task InvokeAsync_runs_ai_chat_and_generates_draft_preview()
    {
        var dependencies = CreateDependencies();
        BuildAiPlanningRequest? capturedRequest = null;
        var app = CreateApp(
            dependencies,
            runtimePaths: CreateRuntimePaths(),
            startupAction: _ => Task.CompletedTask,
            generateAiPlanningDraft: (request, _) =>
            {
                capturedRequest = request;
                return Task.FromResult(CreateWeeklyDraftResult());
            },
            input: new StringReader("Plan my week.\n/exit\n"));

        var exitCode = await app.InvokeAsync(["ai", "chat", "--mode", "new-weekly"]);

        Assert.Equal(0, exitCode);
        Assert.NotNull(capturedRequest);
        Assert.Equal(AiPlanningMode.NewWeekly, capturedRequest.Mode);
        Assert.Equal("Plan my week.", capturedRequest.UserPrompt);
        var output = dependencies.Output.ToString();
        Assert.Contains("mode: new-weekly", output);
        Assert.Contains("draft ready", output);
        Assert.Contains("summary: Weekly plan.", output);
    }

    [Fact]
    public async Task InvokeAsync_ai_chat_discards_current_draft()
    {
        var dependencies = CreateDependencies();
        var app = CreateApp(
            dependencies,
            runtimePaths: CreateRuntimePaths(),
            startupAction: _ => Task.CompletedTask,
            generateAiPlanningDraft: (_, _) => Task.FromResult(CreateWeeklyDraftResult()),
            input: new StringReader("Plan my week.\n/discard\n/exit\n"));

        var exitCode = await app.InvokeAsync(["ai", "chat", "--mode", "new-weekly"]);

        Assert.Equal(0, exitCode);
        Assert.Contains("draft discarded", dependencies.Output.ToString());
    }

    [Fact]
    public async Task InvokeAsync_ai_chat_sends_previous_turns_as_conversation_history()
    {
        var dependencies = CreateDependencies();
        var capturedRequests = new List<BuildAiPlanningRequest>();
        var app = CreateApp(
            dependencies,
            runtimePaths: CreateRuntimePaths(),
            startupAction: _ => Task.CompletedTask,
            generateAiPlanningResponse: (request, _) =>
            {
                capturedRequests.Add(request);
                if (capturedRequests.Count == 1)
                {
                    return Task.FromResult(new GenerateAiPlanningResponseResult(
                        Draft: null,
                        AssistantMessage: "Roadmap: ownership, borrowing, structs, enums, modules.",
                        ProviderModel: "test-model",
                        ModelRequest: new AiPlanningModelRequest(AiPlanningMode.NewProject, null, [], [])));
                }

                return Task.FromResult(new GenerateAiPlanningResponseResult(
                    CreateProjectDraft(),
                    AssistantMessage: null,
                    ProviderModel: "test-model",
                    ModelRequest: new AiPlanningModelRequest(AiPlanningMode.NewProject, null, [], [])));
            },
            input: new StringReader("Help me think about a Rust roadmap.\nCreate the tasks.\n/exit\n"));

        var exitCode = await app.InvokeAsync(["ai", "chat", "--mode", "new-project"]);

        Assert.Equal(0, exitCode);
        Assert.Equal(2, capturedRequests.Count);
        Assert.Empty(capturedRequests[0].ConversationHistory);
        Assert.False(capturedRequests[0].RequireStructuredDraft);
        Assert.Equal(2, capturedRequests[1].ConversationHistory.Count);
        Assert.True(capturedRequests[1].RequireStructuredDraft);
        Assert.Equal(AiPlanningMessageRole.User, capturedRequests[1].ConversationHistory[0].Role);
        Assert.Equal("Help me think about a Rust roadmap.", capturedRequests[1].ConversationHistory[0].Content);
        Assert.Equal(AiPlanningMessageRole.Assistant, capturedRequests[1].ConversationHistory[1].Role);
        Assert.Contains("ownership", capturedRequests[1].ConversationHistory[1].Content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task InvokeAsync_ai_chat_applies_current_draft_after_confirmation()
    {
        var dependencies = CreateDependencies();
        AiPlanningDraft? appliedDraft = null;
        var app = CreateApp(
            dependencies,
            runtimePaths: CreateRuntimePaths(),
            startupAction: _ => Task.CompletedTask,
            generateAiPlanningDraft: (_, _) => Task.FromResult(CreateWeeklyDraftResult()),
            applyAiPlanningDraft: (draft, _) =>
            {
                appliedDraft = draft;
                return Task.FromResult(new AiPlanningDraftApplyResult(
                [
                    new AiPlanningDraftAppliedAction("create_task", "t-0626-1", "default", "week")
                ]));
            },
            input: new StringReader("Plan my week.\n/apply\nyes\n/exit\n"));

        var exitCode = await app.InvokeAsync(["ai", "chat", "--mode", "new-weekly"]);

        Assert.Equal(0, exitCode);
        Assert.NotNull(appliedDraft);
        Assert.Contains("applied:", dependencies.Output.ToString());
    }

    [Fact]
    public async Task InvokeAsync_writes_parse_error_for_unknown_command()
    {
        var dependencies = CreateDependencies();
        var app = CreateApp(dependencies);

        var exitCode = await app.InvokeAsync(["unknown-command"]);

        Assert.Equal(1, exitCode);
        var errorOutput = dependencies.Error.ToString();
        Assert.False(string.IsNullOrWhiteSpace(errorOutput));
        Assert.True(
            errorOutput.Contains("unrecognized command", StringComparison.OrdinalIgnoreCase)
            || errorOutput.Contains("comando", StringComparison.OrdinalIgnoreCase),
            $"Unexpected error output: {errorOutput}");
    }

    private static TermBulletCliApp CreateApp(
        TestDependencies dependencies,
        Func<CancellationToken, Task>? startupAction = null,
        TermBulletRuntimePaths? runtimePaths = null,
        Func<BuildAiPlanningRequest, CancellationToken, Task<GenerateAiPlanningDraftResult>>? generateAiPlanningDraft = null,
        Func<BuildAiPlanningRequest, CancellationToken, Task<GenerateAiPlanningResponseResult>>? generateAiPlanningResponse = null,
        Func<AiPlanningDraft, CancellationToken, Task<AiPlanningDraftApplyResult>>? applyAiPlanningDraft = null,
<<<<<<< HEAD
        Func<string?, CancellationToken, Task<AiPlanningProviderResponse>>? testAiProfileConnection = null,
=======
        Func<TermBulletConfig, string, CancellationToken, Task<AiPlanningProviderResponse>>? testAiProfileConnection = null,
>>>>>>> 31d6ba16bacfc3554d22ce88aea847e70d502125
        TextReader? input = null)
    {
        return new TermBulletCliApp(
            new ClearStoredHistoryUseCase(
                dependencies.HistoryService,
                new FixedClock(new DateTimeOffset(2026, 4, 23, 12, 0, 0, TimeSpan.Zero))),
            dependencies.Output,
            dependencies.Error,
            runtimePaths: runtimePaths,
            generateAiPlanningDraft: generateAiPlanningDraft,
            generateAiPlanningResponse: generateAiPlanningResponse,
            applyAiPlanningDraft: applyAiPlanningDraft,
            testAiProfileConnection: testAiProfileConnection,
            input: input,
            startupAction: startupAction);
    }

    private static GenerateAiPlanningDraftResult CreateWeeklyDraftResult() =>
        new(
            AiPlanningDraftParser.Parse(
                """
                {
                  "mode": "new_weekly",
                  "summary": "Weekly plan.",
                  "actions": [
                    {
                      "type": "create_task",
                      "tag": "default",
                      "collection": "week",
                      "content": "Review open tasks"
                    }
                  ]
                }
                """),
            "test-model",
            new AiPlanningModelRequest(AiPlanningMode.NewWeekly, null, [], []));

    private static AiPlanningDraft CreateProjectDraft() =>
        AiPlanningDraftParser.Parse(
            """
            {
              "mode": "new_project",
              "summary": "Rust study tasks.",
              "actions": [
                {
                  "type": "create_tag",
                  "name": "estudos-rust"
                },
                {
                  "type": "create_task",
                  "tag": "estudos-rust",
                  "collection": "today",
                  "content": "Start Rust ownership study"
                }
              ]
            }
            """);
<<<<<<< HEAD

=======

>>>>>>> 31d6ba16bacfc3554d22ce88aea847e70d502125
    private static TestDependencies CreateDependencies()
    {
        return new TestDependencies(
            new FakeHistoryMaintenanceService(),
            new StringWriter(new StringBuilder()),
            new StringWriter(new StringBuilder()));
    }

    private TermBulletRuntimePaths CreateRuntimePaths() =>
        new(
            Path.Combine(_root, "conf.json"),
            Path.Combine(_root, "data-root"),
            Path.Combine(_root, "data-root", "data"));
<<<<<<< HEAD

    private sealed record TestDependencies(
        FakeHistoryMaintenanceService HistoryService,
        StringWriter Output,
        StringWriter Error);

    private sealed class FakeHistoryMaintenanceService : IHistoryMaintenanceService
    {
        public (int Month, int Year)? ClearedMonth { get; private set; }

        public bool ClearAllCalled { get; private set; }

        public Task ClearMonthAsync(int month, int year, CancellationToken cancellationToken = default)
        {
            ClearedMonth = (month, year);
            return Task.CompletedTask;
        }

        public Task ClearAllAsync(CancellationToken cancellationToken = default)
        {
            ClearAllCalled = true;
            return Task.CompletedTask;
        }
    }

=======

    private sealed record TestDependencies(
        FakeHistoryMaintenanceService HistoryService,
        StringWriter Output,
        StringWriter Error);

    private sealed class FakeHistoryMaintenanceService : IHistoryMaintenanceService
    {
        public (int Month, int Year)? ClearedMonth { get; private set; }

        public bool ClearAllCalled { get; private set; }

        public Task ClearMonthAsync(int month, int year, CancellationToken cancellationToken = default)
        {
            ClearedMonth = (month, year);
            return Task.CompletedTask;
        }

        public Task ClearAllAsync(CancellationToken cancellationToken = default)
        {
            ClearAllCalled = true;
            return Task.CompletedTask;
        }
    }

>>>>>>> 31d6ba16bacfc3554d22ce88aea847e70d502125
    private sealed class FixedClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }
}
