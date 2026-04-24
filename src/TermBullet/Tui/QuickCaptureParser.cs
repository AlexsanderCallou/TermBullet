using TermBullet.Application.Items;
using TermBullet.Core.Items;

namespace TermBullet.Tui;

public static class QuickCaptureParser
{
    public static CreateItemRequest Parse(
        string rawInput,
        ItemCollection collection,
        ItemType? forcedType = null)
    {
        if (string.IsNullOrWhiteSpace(rawInput))
        {
            throw new ArgumentException("Capture text is required.", nameof(rawInput));
        }

        var trimmed = rawInput.Trim();
        var parsedType = forcedType ?? ResolveType(trimmed, out trimmed);

        if (forcedType is not null)
        {
            trimmed = StripKnownPrefix(trimmed);
        }

        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new ArgumentException("Capture text is required.", nameof(rawInput));
        }

        return new CreateItemRequest
        {
            Type = parsedType,
            Content = trimmed,
            Collection = collection
        };
    }

    private static ItemType ResolveType(string input, out string content)
    {
        content = StripKnownPrefix(input, out var resolvedType);
        return resolvedType;
    }

    private static string StripKnownPrefix(string input) => StripKnownPrefix(input, out _);

    private static string StripKnownPrefix(string input, out ItemType resolvedType)
    {
        resolvedType = ItemType.Task;

        if (input.Length >= 2 && input[1] == ' ')
        {
            resolvedType = input[0] switch
            {
                '-' => ItemType.Task,
                '.' => ItemType.Note,
                'o' or 'O' => ItemType.Event,
                _ => ItemType.Task
            };

            if (input[0] is '-' or '.' or 'o' or 'O')
            {
                return input[2..].Trim();
            }
        }

        return input.Trim();
    }
}

