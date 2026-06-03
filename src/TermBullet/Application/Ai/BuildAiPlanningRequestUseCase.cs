using System.Text;
using TermBullet.Domain.Items;
using TermBullet.Repositories.Interfaces;
using TermBullet.Services.Ai;

namespace TermBullet.Application.Ai;

public sealed class BuildAiPlanningRequestUseCase(
    IPlanningAgentPromptLoader agentPromptLoader,
    IItemRepository itemRepository)
{
    public async Task<AiPlanningModelRequest> ExecuteAsync(
        BuildAiPlanningRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userPrompt = NormalizeUserPrompt(request.UserPrompt);
        var tag = NormalizeTag(request.Tag);
        if (request.Mode == AiPlanningMode.ReviseProject && tag is null)
        {
            throw new ArgumentException("Tag is required for revise_project planning.", "tag");
        }

        var agentPrompt = await agentPromptLoader.LoadAsync(cancellationToken);
        var contextItems = await BuildContextAsync(request.Mode, tag, cancellationToken);
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

        if (contextItems.Count > 0)
        {
            messages.Add(new AiPlanningMessage(
                AiPlanningMessageRole.Context,
                BuildContextMessage(request.Mode, tag, contextItems)));
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
            ? (modeKey is "new_weekly" or "revise_weekly" ? "default" : "<project-tag>")
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
                "collection": "today|week|month|backlog",
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
        - move_task: { "type": "move_task", "public_ref": "<existing-task-ref>", "collection": "today|week|month|backlog" }
        - set_priority: { "type": "set_priority", "public_ref": "<existing-task-ref>", "priority": "none|low|medium|high" }
        - cancel_task: { "type": "cancel_task", "public_ref": "<existing-task-ref>" }
        rules:
        - Return only one final response envelope JSON object.
        - Replace every placeholder before returning JSON.
        - For {{modeKey}}, draft.mode must be exactly "{{modeKey}}".
        - Do not return the action_templates or rules.
        - Never apply changes; draft_ready only means "ready for TermBullet approval".
        """;
    }

    private async Task<IReadOnlyList<AiPlanningContextItem>> BuildContextAsync(
        AiPlanningMode mode,
        string? tag,
        CancellationToken cancellationToken)
    {
        if (mode is AiPlanningMode.NewProject or AiPlanningMode.NewWeekly)
        {
            return [];
        }

        var items = await itemRepository.ListAsync(status: ItemStatus.Open, cancellationToken: cancellationToken);
        var filtered = mode switch
        {
            AiPlanningMode.ReviseWeekly => items.Where(item =>
                string.Equals(item.Tag, "default", StringComparison.OrdinalIgnoreCase)),
            AiPlanningMode.ReviseProject => items.Where(item =>
                string.Equals(item.Tag, tag, StringComparison.OrdinalIgnoreCase)),
            _ => []
        };

        return filtered
            .OrderBy(item => item.Collection)
            .ThenBy(item => item.PublicRef.Value, StringComparer.OrdinalIgnoreCase)
            .Select(ToContextItem)
            .ToArray();
    }

    private static AiPlanningContextItem ToContextItem(Item item) =>
        new(
            item.PublicRef.Value,
            item.Type,
            item.Status,
            item.Collection,
            item.Content,
            item.Description,
            item.Tag);

    private static string BuildContextMessage(
        AiPlanningMode mode,
        string? tag,
        IReadOnlyList<AiPlanningContextItem> items)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"mode: {ToModeKey(mode)}");
        if (!string.IsNullOrWhiteSpace(tag))
        {
            builder.AppendLine($"tag: {tag}");
        }

        builder.AppendLine("context_items:");
        foreach (var item in items)
        {
            builder.Append("- ");
            builder.Append(item.PublicRef);
            builder.Append(" | ");
            builder.Append(item.Type.ToString().ToLowerInvariant());
            builder.Append(" | ");
            builder.Append(item.Status.ToString().ToLowerInvariant());
            builder.Append(" | ");
            builder.Append(item.Collection.ToString().ToLowerInvariant());
            builder.Append(" | ");
            builder.Append(item.Tag);
            builder.Append(" | ");
            builder.AppendLine(item.Content);
            if (!string.IsNullOrWhiteSpace(item.Description))
            {
                builder.AppendLine($"  description: {item.Description}");
            }
        }

        return builder.ToString().TrimEnd();
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
            AiPlanningMode.ReviseWeekly => "revise_weekly",
            AiPlanningMode.ReviseProject => "revise_project",
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unsupported AI planning mode.")
        };
}
