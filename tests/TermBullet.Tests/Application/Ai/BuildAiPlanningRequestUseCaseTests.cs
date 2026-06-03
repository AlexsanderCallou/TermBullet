using TermBullet.Application.Ai;
using TermBullet.Domain.Items;
using TermBullet.Domain.Refs;
using TermBullet.Repositories.Interfaces;
using TermBullet.Services.Ai;

namespace TermBullet.Tests.Application.Ai;

public sealed class BuildAiPlanningRequestUseCaseTests
{
    private static readonly DateTimeOffset Now = new(2026, 4, 23, 10, 30, 0, TimeSpan.Zero);

    [Fact]
    public async Task ExecuteAsync_builds_new_project_request_with_agent_mode_context_and_user_prompt()
    {
        var repository = new FakeItemRepository(
            [
                CreateTask(1, "Existing auth task", "auth"),
                CreateTask(2, "Existing java task", "estudo-java")
            ]);
        var agentLoader = new FakePlanningAgentPromptLoader("agent prompt");
        var useCase = new BuildAiPlanningRequestUseCase(agentLoader, repository);

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
        Assert.Equal("Plan Java studies with tag estudo-java.", request.Messages[2].Content);
        Assert.Empty(request.ContextItems);
        Assert.Equal(0, repository.ListCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_builds_new_weekly_template_with_default_tag()
    {
        var useCase = new BuildAiPlanningRequestUseCase(
            new FakePlanningAgentPromptLoader("agent prompt"),
            new FakeItemRepository([]));

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
            new FakePlanningAgentPromptLoader("agent prompt"),
            new FakeItemRepository([]));

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
    public async Task ExecuteAsync_builds_revise_project_request_with_only_selected_tag_context()
    {
        var repository = new FakeItemRepository(
            [
                CreateTask(1, "Map nutrition formulas", "chatbot-nutricional"),
                CreateTask(2, "Build auth flow", "auth"),
                CreateNote(1, "Nutrition chatbot scope", "chatbot-nutricional")
            ]);
        var agentLoader = new FakePlanningAgentPromptLoader("agent prompt");
        var useCase = new BuildAiPlanningRequestUseCase(agentLoader, repository);

        var request = await useCase.ExecuteAsync(new BuildAiPlanningRequest
        {
            Mode = AiPlanningMode.ReviseProject,
            Tag = "chatbot-nutricional",
            UserPrompt = "Suggest next steps."
        });

        Assert.Equal(AiPlanningMode.ReviseProject, request.Mode);
        Assert.Equal(2, request.ContextItems.Count);
        Assert.All(request.ContextItems, item => Assert.Equal("chatbot-nutricional", item.Tag));
        Assert.Contains(request.Messages, message =>
            message.Role == AiPlanningMessageRole.Context
            && message.Content.Contains("Map nutrition formulas", StringComparison.Ordinal));
        Assert.Contains(request.Messages, message =>
            message.Role == AiPlanningMessageRole.User
            && message.Content == "Suggest next steps.");
    }

    [Fact]
    public async Task ExecuteAsync_rejects_revise_project_without_tag()
    {
        var useCase = new BuildAiPlanningRequestUseCase(
            new FakePlanningAgentPromptLoader("agent prompt"),
            new FakeItemRepository([]));

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.ExecuteAsync(new BuildAiPlanningRequest
            {
                Mode = AiPlanningMode.ReviseProject,
                UserPrompt = "Review project."
            }));

        Assert.Equal("tag", exception.ParamName);
    }

    [Fact]
    public async Task ExecuteAsync_rejects_empty_user_prompt()
    {
        var useCase = new BuildAiPlanningRequestUseCase(
            new FakePlanningAgentPromptLoader("agent prompt"),
            new FakeItemRepository([]));

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.ExecuteAsync(new BuildAiPlanningRequest
            {
                Mode = AiPlanningMode.NewWeekly,
                UserPrompt = " "
            }));

        Assert.Equal("userPrompt", exception.ParamName);
    }

    private static Item CreateTask(int sequence, string content, string tag)
    {
        return Item.Create(
            Guid.NewGuid(),
            PublicRef.Create(ItemType.Task, 4, 2026, sequence),
            ItemType.Task,
            content,
            ItemCollection.Today,
            Now,
            tag: tag);
    }

    private static Item CreateNote(int sequence, string content, string tag)
    {
        return Item.Create(
            Guid.NewGuid(),
            PublicRef.Create(ItemType.Note, 4, 2026, sequence),
            ItemType.Note,
            content,
            ItemCollection.Notes,
            Now,
            tag: tag);
    }

    private sealed class FakePlanningAgentPromptLoader(string prompt) : IPlanningAgentPromptLoader
    {
        public Task<string> LoadAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(prompt);
    }

    private sealed class FakeItemRepository(IReadOnlyCollection<Item> items) : IItemRepository
    {
        public int ListCallCount { get; private set; }

        public Task<int> GetCurrentPublicRefSequenceAsync(ItemType type, int month, int year, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<bool> PublicRefExistsAsync(string publicRef, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task AddAsync(Item item, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task UpdateAsync(Item item, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task DeleteByPublicRefAsync(string publicRef, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task ClearHistoryAsync(CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<IReadOnlyCollection<Item>> ListAsync(
            ItemCollection? collection = null,
            ItemStatus? status = null,
            CancellationToken cancellationToken = default)
        {
            ListCallCount++;
            var query = items.AsEnumerable();
            if (collection is not null)
            {
                query = query.Where(item => item.Collection == collection.Value);
            }

            if (status is not null)
            {
                query = query.Where(item => item.Status == status.Value);
            }

            return Task.FromResult<IReadOnlyCollection<Item>>(query.ToArray());
        }

        public Task<Item?> FindByPublicRefAsync(string publicRef, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }
}
