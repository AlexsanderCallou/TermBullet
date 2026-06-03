using System.Text.Json;

namespace TermBullet.Application.Ai;

public static class AiPlanningDraftParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    public static AiPlanningDraft Parse(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new InvalidOperationException("AI planning draft is empty.");
        }

        try
        {
            return JsonSerializer.Deserialize<AiPlanningDraft>(json, JsonOptions)
                ?? throw new InvalidOperationException("AI planning draft is empty.");
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException("AI planning draft is malformed JSON.", exception);
        }
    }
}
