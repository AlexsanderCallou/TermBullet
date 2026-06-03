using TermBullet.Application.Ai;
using TermBullet.Services.Ai;

namespace TermBullet.Tests.Application.Ai;

public sealed class BuildAiPlanningRequestUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_builds_new_project_request_with_agent_mode_context_and_user_prompt()
    {
        var agentLoader = new FakePlanningAgentPromptLoader("agent prompt");
        var useCase = new BuildAiPlanningRequestUseCase(agentLoader);

        var request = await useCase.ExecuteAsync(new BuildAiPlanningRequest
        {
            Mode = AiPlanningMode.NewProject,
            UserPrompt = "Plan Java studies with tag estudo-java."
        });

        Assert.Equal(AiPlanningMode.NewProject, request.Mode);
        Assert.Equal(
            [AiPlanningMessageRole.Agent, AiPlanningMessageRole.Context, AiPlanningMessageRole.User],
            request.Messages.Select(message => message.Role));
        Assert.Equal("agent prompt", request.Messages[0].Content);
        Assert.Contains("requested_mode: new_project", request.Messages[1].Content, StringComparison.Ordinal);
        Assert.Contains("response_envelope_template:", request.Messages[1].Content, StringComparison.Ordinal);
        Assert.Contains("\"draft_ready\": false", request.Messages[1].Content, StringComparison.Ordinal);
        Assert.Contains("\"mode\": \"new_project\"", request.Messages[1].Content, StringComparison.Ordinal);
        Assert.Contains("\"tag\": \"<project-tag>\"", request.Messages[1].Content, StringComparison.Ordinal);
        Assert.Contains("\"collection\": \"today\"", request.Messages[1].Content, StringComparison.Ordinal);
        Assert.DoesNotContain("\"collection\": \"today|week|month|backlog\"", request.Messages[1].Content, StringComparison.Ordinal);
        Assert.Equal("Plan Java studies with tag estudo-java.", request.Messages[2].Content);
        Assert.Empty(request.ContextItems);
    }

    [Fact]
    public async Task ExecuteAsync_builds_new_weekly_template_with_default_tag()
    {
        var useCase = new BuildAiPlanningRequestUseCase(
            new FakePlanningAgentPromptLoader("agent prompt"));

        var request = await useCase.ExecuteAsync(new BuildAiPlanningRequest
        {
            Mode = AiPlanningMode.NewWeekly,
            UserPrompt = "Plan my week."
        });

        Assert.Contains("response_envelope_template:", request.Messages[1].Content, StringComparison.Ordinal);
        Assert.Contains("\"mode\": \"new_weekly\"", request.Messages[1].Content, StringComparison.Ordinal);
        Assert.Contains("\"tag\": \"default\"", request.Messages[1].Content, StringComparison.Ordinal);
        Assert.Contains("Replace every placeholder before returning JSON.", request.Messages[1].Content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExecuteAsync_adds_conversation_history_before_current_user_prompt()
    {
        var useCase = new BuildAiPlanningRequestUseCase(
            new FakePlanningAgentPromptLoader("agent prompt"));

        var request = await useCase.ExecuteAsync(new BuildAiPlanningRequest
        {
            Mode = AiPlanningMode.NewProject,
            UserPrompt = "Create the tasks.",
            RequireStructuredDraft = false,
            ConversationHistory =
            [
                new(AiPlanningMessageRole.User, "Create a Rust study roadmap with tag estudos-rust."),
                new(AiPlanningMessageRole.Assistant, "Roadmap: learn ownership, borrowing, structs, enums, and modules.")
            ]
        });

        Assert.Equal(AiPlanningMessageRole.User, request.Messages[^3].Role);
        Assert.Equal("Create a Rust study roadmap with tag estudos-rust.", request.Messages[^3].Content);
        Assert.Equal(AiPlanningMessageRole.Assistant, request.Messages[^2].Role);
        Assert.Contains("ownership", request.Messages[^2].Content, StringComparison.Ordinal);
        Assert.Equal(AiPlanningMessageRole.User, request.Messages[^1].Role);
        Assert.Equal("Create the tasks.", request.Messages[^1].Content);
    }

    [Fact]
    public async Task ExecuteAsync_rejects_empty_user_prompt()
    {
        var useCase = new BuildAiPlanningRequestUseCase(
            new FakePlanningAgentPromptLoader("agent prompt"));

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.ExecuteAsync(new BuildAiPlanningRequest
            {
                Mode = AiPlanningMode.NewWeekly,
                UserPrompt = " "
            }));

        Assert.Equal("userPrompt", exception.ParamName);
    }

    private sealed class FakePlanningAgentPromptLoader(string prompt) : IPlanningAgentPromptLoader
    {
        public Task<string> LoadAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(prompt);
    }
}
