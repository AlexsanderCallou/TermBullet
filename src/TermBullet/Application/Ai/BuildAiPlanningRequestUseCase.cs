using System.Text;
using TermBullet.Services.Ai;

namespace TermBullet.Application.Ai;

public sealed class BuildAiPlanningRequestUseCase(
    IPlanningAgentPromptLoader agentPromptLoader)
{
    public async Task<AiPlanningModelRequest> ExecuteAsync(
        BuildAiPlanningRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userPrompt = NormalizeUserPrompt(request.UserPrompt);
        var tag = NormalizeTag(request.Tag);

        var agentPrompt = await agentPromptLoader.LoadAsync(cancellationToken);
        IReadOnlyList<AiPlanningContextItem> contextItems = [];
        var messages = new List<AiPlanningMessage>
        {
            new(AiPlanningMessageRole.Agent, agentPrompt)
        };

        messages.Add(new AiPlanningMessage(
            AiPlanningMessageRole.Context,
            BuildRequestControlMessage(request.Mode, tag, request.RequireStructuredDraft)));

        if (!request.RequireStructuredDraft)
        {
            messages.Add(new AiPlanningMessage(
                AiPlanningMessageRole.Context,
                "This is an interactive planning conversation. Ask concise clarification questions or discuss the plan in normal text until a draft is ready. Return a JSON draft only when you are ready to propose tasks for approval."));
        }

        foreach (var message in NormalizeConversationHistory(request.ConversationHistory))
        {
            messages.Add(message);
        }

        messages.Add(new AiPlanningMessage(AiPlanningMessageRole.User, userPrompt));

        return new AiPlanningModelRequest(request.Mode, tag, messages, contextItems, request.RequireStructuredDraft);
    }

    private static string BuildRequestControlMessage(
        AiPlanningMode mode,
        string? tag,
        bool requireStructuredDraft)
    {
        var builder = new StringBuilder();
        var modeKey = ToModeKey(mode);
        builder.AppendLine($"requested_mode: {modeKey}");
        builder.AppendLine($"draft_mode_must_equal: {modeKey}");
        if (!string.IsNullOrWhiteSpace(tag))
        {
            builder.AppendLine($"requested_tag: {tag}");
        }

        if (requireStructuredDraft)
        {
            builder.AppendLine("response_contract: return exactly one TermBullet response envelope JSON object with draft_ready=true and a filled draft. Do not add markdown, explanations, or placeholder text.");
        }
        else
        {
            builder.AppendLine("response_contract: always return exactly one TermBullet response envelope JSON object. Use draft_ready=false for chat and draft_ready=true only when a draft is ready for user approval.");
        }

        builder.AppendLine("response_envelope_template:");
        builder.AppendLine(BuildResponseEnvelopeTemplate(modeKey, tag));

        return builder.ToString().TrimEnd();
    }

    private static string BuildResponseEnvelopeTemplate(string modeKey, string? tag)
    {
        var draftTag = string.IsNullOrWhiteSpace(tag)
            ? (modeKey is "new_weekly" ? "default" : "<project-tag>")
            : tag;

        return $$"""
        {
          "kind": "chat|draft",
          "message": "<short user-facing message>",
          "draft_ready": false,
          "draft": null
        }
        When draft_ready is true, use this draft shape:
        {
          "kind": "draft",
          "message": "Draft ready for approval.",
          "draft_ready": true,
          "draft": {
            "mode": "{{modeKey}}",
            "summary": "<one sentence user-facing plan summary>",
            "actions": [
              {
                "type": "create_task",
                "tag": "{{draftTag}}",
                "collection": "today",
                "content": "<short actionable task>",
                "description": "<optional task detail>",
                "priority": "none|low|medium|high"
              }
            ]
          }
        }
        action_templates:
        - create_tag: { "type": "create_tag", "name": "<non-default-tag>" }
        - create_note: { "type": "create_note", "tag": "{{draftTag}}", "content": "<note title>", "description": "<note body>" }
        rules:
        - Return only one final response envelope JSON object.
        - Replace every placeholder before returning JSON.
        - For {{modeKey}}, draft.mode must be exactly "{{modeKey}}".
        - For create_task, collection must be exactly one of: "today", "week", "month", "backlog".
        - Never return combined placeholder values such as "today|week|month|backlog".
        - Do not return the action_templates or rules.
        - Never apply changes; draft_ready only means "ready for TermBullet approval".
        """;
    }

    private static string NormalizeUserPrompt(string? userPrompt)
    {
        if (string.IsNullOrWhiteSpace(userPrompt))
        {
            throw new ArgumentException("User prompt must not be empty.", "userPrompt");
        }

        return userPrompt.Trim();
    }

    private static string? NormalizeTag(string? tag) =>
        string.IsNullOrWhiteSpace(tag) ? null : tag.Trim();

    private static IReadOnlyList<AiPlanningMessage> NormalizeConversationHistory(
        IReadOnlyList<AiPlanningMessage>? history)
    {
        if (history is null || history.Count == 0)
        {
            return [];
        }

        return history
            .Where(message => message.Role is AiPlanningMessageRole.User or AiPlanningMessageRole.Assistant)
            .Select(message => new AiPlanningMessage(message.Role, message.Content.Trim()))
            .Where(message => message.Content.Length > 0)
            .TakeLast(12)
            .ToArray();
    }

    private static string ToModeKey(AiPlanningMode mode) =>
        mode switch
        {
            AiPlanningMode.NewProject => "new_project",
            AiPlanningMode.NewWeekly => "new_weekly",
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unsupported AI planning mode.")
        };
}
