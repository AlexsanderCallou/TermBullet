using TermBullet.Application.Ai;

namespace TermBullet.Tui.Screens;

public static class PlanningConversationFormatter
{
    private const int DefaultMaxLineLength = 96;

    public static IReadOnlyList<string> Format(
        IReadOnlyList<string> lines,
        GenerateAiPlanningDraftResult? currentDraft,
        int maxLineLength = DefaultMaxLineLength)
    {
        var formatted = new List<string>();
        foreach (var line in lines)
        {
            formatted.AddRange(WrapLine(line, maxLineLength));
        }

        if (currentDraft is null)
        {
            return formatted;
        }

        formatted.AddRange(WrapLine($"draft> {currentDraft.Draft.Summary}", maxLineLength));
        foreach (var action in currentDraft.Draft.Actions.Take(8))
        {
            formatted.AddRange(WrapLine($"draft> {FormatAction(action)}", maxLineLength));
        }

        return formatted;
    }

    private static string FormatAction(AiPlanningDraftAction action)
    {
        var parts = new[]
        {
            action.Type,
            action.Collection,
            action.Tag,
            action.Content ?? action.Name ?? action.PublicRef
        };

        return string.Join(" ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
    }

    private static IEnumerable<string> WrapLine(string line, int maxLineLength)
    {
        if (maxLineLength < 20)
        {
            maxLineLength = 20;
        }

        foreach (var physicalLine in line.Replace("\r\n", "\n").Split('\n'))
        {
            foreach (var wrapped in WrapPhysicalLine(physicalLine, maxLineLength))
            {
                yield return wrapped;
            }
        }
    }

    private static IEnumerable<string> WrapPhysicalLine(string line, int maxLineLength)
    {
        if (line.Length <= maxLineLength)
        {
            yield return line;
            yield break;
        }

        var prefixLength = GetPrefixLength(line);
        var continuationPrefix = prefixLength > 0
            ? new string(' ', prefixLength)
            : string.Empty;
        var currentPrefix = string.Empty;
        var remaining = line;

        while (remaining.Length > maxLineLength - currentPrefix.Length)
        {
            var available = maxLineLength - currentPrefix.Length;
            var breakAt = remaining.LastIndexOf(' ', available);
            if (breakAt <= 0)
            {
                breakAt = available;
            }

            yield return currentPrefix + remaining[..breakAt].TrimEnd();
            remaining = remaining[breakAt..].TrimStart();
            currentPrefix = continuationPrefix;
        }

        if (remaining.Length > 0)
        {
            yield return currentPrefix + remaining;
        }
    }

    private static int GetPrefixLength(string line)
    {
        var marker = line.IndexOf("> ", StringComparison.Ordinal);
        return marker >= 0 ? marker + 2 : 0;
    }
}
