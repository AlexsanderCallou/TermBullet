using TermBullet.Application.Items;
using TermBullet.Domain.Items;

namespace TermBullet.Tui.Screens;

public sealed class AddItemFormDraft
{
    public ItemType Type { get; set; } = ItemType.Task;

    public AddItemTimingChoice Timing { get; set; } = AddItemTimingChoice.Today;

    public string Content { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Priority Priority { get; set; } = TermBullet.Domain.Items.Priority.None;

    public string TagsText { get; set; } = string.Empty;

    public string PlannedForText { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(1)).ToString("yyyy-MM-dd");

    public static CreateItemRequest BuildQuickTaskRequest(string content)
    {
        return new CreateItemRequest
        {
            Type = ItemType.Task,
            Content = NormalizeRequiredText(content, nameof(content)),
            Collection = ItemCollection.Today,
            PlannedFor = DateOnly.FromDateTime(DateTime.Today)
        };
    }

    public CreateItemRequest BuildRequest()
    {
        var content = NormalizeRequiredText(Content, nameof(Content));
        var description = NormalizeOptionalText(Description);
        var tags = ParseTags(TagsText);
        var collection = ResolveCollection();
        var plannedAt = ResolvePlannedDate();

        return new CreateItemRequest
        {
            Type = Type,
            Content = content,
            Collection = collection,
            Description = description,
            Priority = Type == ItemType.Task ? Priority : TermBullet.Domain.Items.Priority.None,
            Tags = tags.Count > 0 ? tags : null,
            PlannedFor = Type == ItemType.Task ? plannedAt : null,
            ScheduledAt = Type == ItemType.Event && plannedAt is not null ? ToUtcInstant(plannedAt.Value) : null
        };
    }

    public IReadOnlyList<string> BuildPreviewLines()
    {
        var plannedAt = ResolvePlannedDate();
        var planningLine = Type == ItemType.Event
            ? plannedAt is null
                ? "scheduled_at: -"
                : $"scheduled_at: {plannedAt.Value:yyyy-MM-dd}"
            : plannedAt is null
                ? "planned_for: -"
                : $"planned_for: {plannedAt.Value:yyyy-MM-dd}";
        var content = string.IsNullOrWhiteSpace(Content) ? "(required)" : Content.Trim();
        var description = string.IsNullOrWhiteSpace(Description) ? "-" : Description.Trim();

        return
        [
            $"type: {Type.ToString().ToLowerInvariant()}",
            $"collection: {ResolveCollection().ToString().ToLowerInvariant()}",
            $"content: {content}",
            $"description: {description}",
            Type == ItemType.Task ? $"priority: {FormatPriority(Priority)}" : "priority: none",
            planningLine,
            $"tags: {FormatTags(TagsText)}"
        ];
    }

    private ItemCollection ResolveCollection() =>
        Type switch
        {
            ItemType.Note => ItemCollection.Backlog,
            ItemType.Event => ItemCollection.Week,
            _ => Timing switch
            {
                AddItemTimingChoice.Today => ItemCollection.Today,
                AddItemTimingChoice.FutureDate => ItemCollection.Week,
                AddItemTimingChoice.Backlog => ItemCollection.Backlog,
                _ => ItemCollection.Today
            }
        };

    private DateOnly ParsePlannedFor()
    {
        var value = PlannedForText.Trim();
        if (!DateOnly.TryParseExact(
            value,
            "yyyy-MM-dd",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None,
            out var plannedFor))
        {
            throw new ArgumentException("Planned for must use yyyy-MM-dd.", nameof(PlannedForText));
        }

        return plannedFor;
    }

    private DateOnly? ResolvePlannedDate() =>
        Type switch
        {
            ItemType.Note => null,
            ItemType.Event => ParsePlannedFor(),
            _ => Timing switch
            {
                AddItemTimingChoice.Backlog => null,
                AddItemTimingChoice.Today => DateOnly.FromDateTime(DateTime.Today),
                AddItemTimingChoice.FutureDate => ParsePlannedFor(),
                _ => null
            }
        };

    private static DateTimeOffset ToUtcInstant(DateOnly date) =>
        new(date.Year, date.Month, date.Day, 0, 0, 0, TimeSpan.Zero);

    private static string NormalizeRequiredText(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Capture text is required.", parameterName);
        }

        return value.Trim();
    }

    private static string? NormalizeOptionalText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private static List<string> ParseTags(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return
        [
            .. value.Split([',', ';'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Distinct(StringComparer.OrdinalIgnoreCase)
        ];
    }

    private static string FormatTags(string? value)
    {
        var tags = ParseTags(value);
        return tags.Count > 0 ? string.Join(", ", tags) : "-";
    }

    private static string FormatPriority(Priority priority) =>
        priority.ToString().ToLowerInvariant();
}
