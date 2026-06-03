using TermBullet.Services.Ai;

namespace TermBullet.Application.Ai;

public sealed class GenerateAiPlanningResponseUseCase(
    BuildAiPlanningRequestUseCase buildRequestUseCase,
    IAiPlanningProvider provider,
    AiPlanningDraftValidator validator)
{
    public async Task<GenerateAiPlanningResponseResult> ExecuteAsync(
        BuildAiPlanningRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var modelRequest = await buildRequestUseCase.ExecuteAsync(request, cancellationToken);
        var providerResponse = await provider.SendAsync(modelRequest, cancellationToken);
        var (draft, assistantMessage) = AiPlanningResponseParser.ParseFlexible(providerResponse.Content);
        if (modelRequest.RequireStructuredDraft && draft is null)
        {
            var repairRequest = BuildStructuredDraftRepairRequest(modelRequest, providerResponse.Content);
            providerResponse = await provider.SendAsync(repairRequest, cancellationToken);
            (draft, assistantMessage) = AiPlanningResponseParser.ParseFlexible(providerResponse.Content);
            if (draft is null)
            {
                throw new InvalidOperationException("AI planning response did not include a structured draft.");
            }
        }

        if (draft is not null)
        {
            var expectedMode = ToModeKey(modelRequest.Mode);
            draft = NormalizeDraftMode(draft, expectedMode);

            var validation = validator.Validate(draft);
            if (!validation.IsValid)
            {
                if (modelRequest.RequireStructuredDraft)
                {
                    var repairRequest = BuildInvalidDraftRepairRequest(
                        modelRequest,
                        providerResponse.Content,
                        validation.Errors);
                    providerResponse = await provider.SendAsync(repairRequest, cancellationToken);
                    (draft, assistantMessage) = AiPlanningResponseParser.ParseFlexible(providerResponse.Content);
                    if (draft is null)
                    {
                        throw new InvalidOperationException("AI planning response did not include a structured draft.");
                    }

                    draft = NormalizeDraftMode(draft, expectedMode);
                    validation = validator.Validate(draft);
                }

                if (!validation.IsValid)
                {
                    throw new InvalidOperationException(
                        $"AI planning draft is invalid: {string.Join(" ", validation.Errors)}");
                }
            }
        }

        return new GenerateAiPlanningResponseResult(
            draft,
            assistantMessage,
            providerResponse.Model ?? string.Empty,
            modelRequest);
    }

    private static string ToModeKey(AiPlanningMode mode) =>
        mode switch
        {
            AiPlanningMode.NewProject => "new_project",
            AiPlanningMode.NewWeekly => "new_weekly",
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unsupported AI planning mode.")
        };

    private static AiPlanningDraft NormalizeDraftMode(AiPlanningDraft draft, string expectedMode) =>
        string.Equals(draft.Mode?.Trim(), expectedMode, StringComparison.OrdinalIgnoreCase)
            ? draft
            : new AiPlanningDraft
            {
                Mode = expectedMode,
                Summary = draft.Summary,
                Actions = draft.Actions
            };

    private static AiPlanningModelRequest BuildStructuredDraftRepairRequest(
        AiPlanningModelRequest originalRequest,
        string previousContent)
    {
        var messages = originalRequest.Messages
            .Concat(
            [
                new AiPlanningMessage(AiPlanningMessageRole.Assistant, previousContent),
                new AiPlanningMessage(
                    AiPlanningMessageRole.User,
                    "The previous response was not a valid TermBullet response envelope with a draft. Return only one filled response envelope JSON object using response_envelope_template. Set draft_ready=true and include draft.mode, draft.summary, and draft.actions. Do not ask questions, do not explain, do not use markdown.")
            ])
            .ToArray();

        return originalRequest with
        {
            Messages = messages,
            RequireStructuredDraft = true
        };
    }

    private static AiPlanningModelRequest BuildInvalidDraftRepairRequest(
        AiPlanningModelRequest originalRequest,
        string previousContent,
        IReadOnlyList<string> validationErrors)
    {
        var messages = originalRequest.Messages
            .Concat(
            [
                new AiPlanningMessage(AiPlanningMessageRole.Assistant, previousContent),
                new AiPlanningMessage(
                    AiPlanningMessageRole.User,
                    $"The previous draft JSON was parsed but failed TermBullet validation: {string.Join(" ", validationErrors)} Return only one corrected response envelope JSON object. Replace all placeholders. For create_task, collection must be exactly one of: today, week, month, backlog. Do not return combined values like today|week|month|backlog. Do not ask questions, do not explain, do not use markdown.")
            ])
            .ToArray();

        return originalRequest with
        {
            Messages = messages,
            RequireStructuredDraft = true
        };
    }
}
