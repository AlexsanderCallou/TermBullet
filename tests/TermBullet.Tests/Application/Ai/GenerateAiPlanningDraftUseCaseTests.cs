using TermBullet.Application.Ai;
using TermBullet.Domain.Items;
using TermBullet.Repositories.Interfaces;
using TermBullet.Services.Ai;

namespace TermBullet.Tests.Application.Ai;

public sealed class GenerateAiPlanningDraftUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_builds_request_calls_provider_and_returns_validated_draft()
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
            """,
            "test-model");
        var useCase = CreateUseCase(provider);

        var result = await useCase.ExecuteAsync(new BuildAiPlanningRequest
        {
            Mode = AiPlanningMode.NewWeekly,
            UserPrompt = "Plan my week."
        });

        Assert.Equal("test-model", result.ProviderModel);
        Assert.Equal("new_weekly", result.Draft.Mode);
        Assert.Single(result.Draft.Actions);
        Assert.Equal(AiPlanningMode.NewWeekly, provider.LastRequest?.Mode);
        Assert.Equal(
            [AiPlanningMessageRole.Agent, AiPlanningMessageRole.Context, AiPlanningMessageRole.User],
            provider.LastRequest?.Messages.Select(message => message.Role));
    }

    [Fact]
    public async Task ExecuteAsync_normalizes_draft_mode_to_requested_mode_before_validation()
    {
        var useCase = CreateUseCase(new FakeAiPlanningProvider(
            """
            {
              "mode": "new_weekly",
              "summary": "Java study project.",
              "actions": [
                {
                  "type": "create_tag",
                  "name": "estudos-java"
                },
                {
                  "type": "create_task",
                  "tag": "estudos-java",
                  "collection": "week",
                  "content": "Practice Java loops"
                }
              ]
            }
            """));

        var result = await useCase.ExecuteAsync(new BuildAiPlanningRequest
        {
            Mode = AiPlanningMode.NewProject,
            UserPrompt = "Plan Java studies."
        });

        Assert.Equal("new_project", result.Draft.Mode);
        Assert.Equal("estudos-java", result.Draft.Actions[1].Tag);
    }

    [Fact]
    public async Task ExecuteAsync_rejects_malformed_provider_json()
    {
        var useCase = CreateUseCase(new FakeAiPlanningProvider("{"));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(new BuildAiPlanningRequest
            {
                Mode = AiPlanningMode.NewWeekly,
                UserPrompt = "Plan my week."
            }));

        Assert.Contains("structured draft", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_rejects_invalid_provider_draft()
    {
        var useCase = CreateUseCase(new FakeAiPlanningProvider(
            """
            {
              "mode": "new_weekly",
              "summary": "Weekly plan.",
              "actions": [
                {
                  "type": "create_task",
                  "tag": "project",
                  "collection": "week",
                  "content": "Invalid tag"
                }
              ]
            }
            """));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(new BuildAiPlanningRequest
            {
                Mode = AiPlanningMode.NewWeekly,
                UserPrompt = "Plan my week."
            }));

        Assert.Contains("invalid", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("new_weekly", exception.Message, StringComparison.Ordinal);
    }

    private static GenerateAiPlanningDraftUseCase CreateUseCase(FakeAiPlanningProvider provider) =>
        new(
            new BuildAiPlanningRequestUseCase(
                new FakePlanningAgentPromptLoader("agent prompt"),
                new FakeItemRepository()),
            provider,
            new AiPlanningDraftValidator());

    private sealed class FakeAiPlanningProvider(string content, string? model = null) : IAiPlanningProvider
    {
        public AiPlanningModelRequest? LastRequest { get; private set; }

        public Task<AiPlanningProviderResponse> SendAsync(
            AiPlanningModelRequest request,
            CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult(new AiPlanningProviderResponse(content, model));
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
