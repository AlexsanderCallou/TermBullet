using TermBullet.Application.Ai;
using TermBullet.Domain.Items;
using TermBullet.Repositories.Interfaces;
using TermBullet.Services.Ai;

namespace TermBullet.Tests.Application.Ai;

public sealed class GenerateAiPlanningResponseUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_returns_conversational_message_without_requiring_draft()
    {
        var provider = new FakeAiPlanningProvider("Tell me the deadline and definition of done.");
        var useCase = CreateUseCase(provider);

        var result = await useCase.ExecuteAsync(new BuildAiPlanningRequest
        {
            Mode = AiPlanningMode.NewProject,
            UserPrompt = "Plan my project.",
            RequireStructuredDraft = false
        });

        Assert.Null(result.Draft);
        Assert.Equal("Tell me the deadline and definition of done.", result.AssistantMessage);
        Assert.False(provider.LastRequest?.RequireStructuredDraft);
    }

    [Fact]
    public async Task ExecuteAsync_returns_draft_when_response_is_structured_json()
    {
        var provider = new FakeAiPlanningProvider(
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
            """);
        var useCase = CreateUseCase(provider);

        var result = await useCase.ExecuteAsync(new BuildAiPlanningRequest
        {
            Mode = AiPlanningMode.NewWeekly,
            UserPrompt = "Plan my week.",
            RequireStructuredDraft = false
        });

        Assert.NotNull(result.Draft);
        Assert.Null(result.AssistantMessage);
        Assert.Single(result.Draft.Actions);
    }

    [Fact]
    public async Task ExecuteAsync_repairs_required_draft_when_first_response_is_chat()
    {
        var provider = new FakeAiPlanningProvider(
            "Sure, I can create those tasks. Here is the plan in prose first.",
            """
            {
              "mode": "new_project",
              "summary": "Rust study roadmap.",
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
        var useCase = CreateUseCase(provider);

        var result = await useCase.ExecuteAsync(new BuildAiPlanningRequest
        {
            Mode = AiPlanningMode.NewProject,
            UserPrompt = "crie as tasks",
            RequireStructuredDraft = true
        });

        Assert.NotNull(result.Draft);
        Assert.Equal(2, provider.Requests.Count);
        Assert.Contains(provider.Requests[1].Messages, message =>
            message.Role == AiPlanningMessageRole.Assistant
            && message.Content.Contains("prose", StringComparison.Ordinal));
        Assert.Contains(provider.Requests[1].Messages, message =>
            message.Role == AiPlanningMessageRole.User
            && message.Content.Contains("response envelope JSON object", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ExecuteAsync_repairs_required_draft_when_first_draft_copies_collection_placeholder()
    {
        var provider = new FakeAiPlanningProvider(
            """
            {
              "mode": "new_project",
              "summary": "Rust study roadmap.",
              "actions": [
                {
                  "type": "create_tag",
                  "name": "estudos-rust"
                },
                {
                  "type": "create_task",
                  "tag": "estudos-rust",
                  "collection": "today|week|month|backlog",
                  "content": "1. Start Rust ownership study"
                }
              ]
            }
            """,
            """
            {
              "mode": "new_project",
              "summary": "Rust study roadmap.",
              "actions": [
                {
                  "type": "create_tag",
                  "name": "estudos-rust"
                },
                {
                  "type": "create_task",
                  "tag": "estudos-rust",
                  "collection": "today",
                  "content": "1. Start Rust ownership study"
                }
              ]
            }
            """);
        var useCase = CreateUseCase(provider);

        var result = await useCase.ExecuteAsync(new BuildAiPlanningRequest
        {
            Mode = AiPlanningMode.NewProject,
            UserPrompt = "Create Rust study tasks.",
            RequireStructuredDraft = true
        });

        Assert.NotNull(result.Draft);
        Assert.Equal("today", result.Draft.Actions[1].Collection);
        Assert.Equal(2, provider.Requests.Count);
        Assert.Contains(provider.Requests[1].Messages, message =>
            message.Role == AiPlanningMessageRole.User
            && message.Content.Contains("today|week|month|backlog", StringComparison.Ordinal));
    }

    private static GenerateAiPlanningResponseUseCase CreateUseCase(FakeAiPlanningProvider provider) =>
        new(
            new BuildAiPlanningRequestUseCase(
                new FakePlanningAgentPromptLoader("agent prompt")),
            provider,
            new AiPlanningDraftValidator());

    private sealed class FakeAiPlanningProvider(params string[] contents) : IAiPlanningProvider
    {
        public AiPlanningModelRequest? LastRequest { get; private set; }
        public List<AiPlanningModelRequest> Requests { get; } = [];

        public Task<AiPlanningProviderResponse> SendAsync(
            AiPlanningModelRequest request,
            CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            Requests.Add(request);
            var index = Math.Min(Requests.Count - 1, contents.Length - 1);
            return Task.FromResult(new AiPlanningProviderResponse(contents[index], null));
        }
    }

    private sealed class FakePlanningAgentPromptLoader(string prompt) : IPlanningAgentPromptLoader
    {
        public Task<string> LoadAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(prompt);
    }

    private sealed class FakeItemRepository : IItemRepository
    {
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
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<Item>>([]);

        public Task<Item?> FindByPublicRefAsync(string publicRef, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }
}
