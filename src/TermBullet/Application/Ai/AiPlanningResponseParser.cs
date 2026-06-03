using System.Text.Json;

namespace TermBullet.Application.Ai;

public static class AiPlanningResponseParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    private const string InvalidDraftMessage =
        "I received a draft-shaped response, but it was not valid TermBullet draft JSON. Ask for a revised plan.";

    public static (AiPlanningDraft? Draft, string? AssistantMessage) ParseFlexible(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("AI planning response is empty.");
        }

        var trimmed = content.Trim();
        var candidates = ExtractJsonCandidates(trimmed).ToArray();
        if (candidates.Length == 0)
        {
            if (LooksDraftShaped(trimmed))
            {
                return (null, InvalidDraftMessage);
            }

            return (null, trimmed);
        }

        foreach (var candidate in candidates)
        {
            try
            {
                var envelope = JsonSerializer.Deserialize<AiPlanningResponseEnvelope>(candidate, JsonOptions);
                if (envelope is not null && HasEnvelopeShape(envelope))
                {
                    if (envelope.DraftReady && envelope.Draft is not null && HasDraftShape(envelope.Draft))
                    {
                        return (envelope.Draft, null);
                    }

                    return (null, string.IsNullOrWhiteSpace(envelope.Message)
                        ? "AI response did not include a message."
                        : envelope.Message.Trim());
                }
            }
            catch (JsonException)
            {
                // Try draft parsing below.
            }

            try
            {
                var draft = AiPlanningDraftParser.Parse(candidate);
                if (HasDraftShape(draft))
                {
                    return (draft, null);
                }
            }
            catch (InvalidOperationException)
            {
                // Try the next JSON-looking block before falling back to a safe message.
            }
        }

        return (null, InvalidDraftMessage);
    }

    private static bool LooksDraftShaped(string content) =>
        content.Contains("```json", StringComparison.OrdinalIgnoreCase)
        || content.Contains("\"actions\"", StringComparison.OrdinalIgnoreCase)
        || content.Contains("\"draft_ready\"", StringComparison.OrdinalIgnoreCase)
        || content.Contains("\"draft\"", StringComparison.OrdinalIgnoreCase)
        || content.Contains("\"mode\"", StringComparison.OrdinalIgnoreCase);

    private static bool HasEnvelopeShape(AiPlanningResponseEnvelope envelope) =>
        !string.IsNullOrWhiteSpace(envelope.Kind)
        || !string.IsNullOrWhiteSpace(envelope.Message)
        || envelope.DraftReady
        || envelope.Draft is not null;

    private static bool HasDraftShape(AiPlanningDraft draft) =>
        !string.IsNullOrWhiteSpace(draft.Mode)
        || !string.IsNullOrWhiteSpace(draft.Summary)
        || draft.Actions.Count > 0;

    private static IEnumerable<string> ExtractJsonCandidates(string content)
    {
        foreach (var fencedBlock in ExtractFencedBlocks(content))
        {
            foreach (var candidate in ExtractBalancedObjects(fencedBlock))
            {
                yield return candidate;
            }
        }

        foreach (var candidate in ExtractBalancedObjects(content))
        {
            yield return candidate;
        }
    }

    private static IEnumerable<string> ExtractFencedBlocks(string content)
    {
        var searchIndex = 0;
        while (searchIndex < content.Length)
        {
            var fenceStart = content.IndexOf("```", searchIndex, StringComparison.Ordinal);
            if (fenceStart < 0)
            {
                yield break;
            }

            var contentStart = content.IndexOf('\n', fenceStart + 3);
            if (contentStart < 0)
            {
                yield break;
            }

            var fenceEnd = content.IndexOf("```", contentStart + 1, StringComparison.Ordinal);
            if (fenceEnd < 0)
            {
                yield return content[(contentStart + 1)..];
                yield break;
            }

            yield return content[(contentStart + 1)..fenceEnd];
            searchIndex = fenceEnd + 3;
        }
    }

    private static IEnumerable<string> ExtractBalancedObjects(string content)
    {
        var start = -1;
        var depth = 0;
        var inString = false;
        var escaped = false;

        for (var index = 0; index < content.Length; index++)
        {
            var character = content[index];

            if (inString)
            {
                if (escaped)
                {
                    escaped = false;
                    continue;
                }

                if (character == '\\')
                {
                    escaped = true;
                    continue;
                }

                if (character == '"')
                {
                    inString = false;
                }

                continue;
            }

            if (character == '"')
            {
                inString = true;
                continue;
            }

            if (character == '{')
            {
                if (depth == 0)
                {
                    start = index;
                }

                depth++;
                continue;
            }

            if (character != '}' || depth == 0)
            {
                continue;
            }

            depth--;
            if (depth == 0 && start >= 0)
            {
                yield return content[start..(index + 1)];
                start = -1;
            }
        }
    }
}
