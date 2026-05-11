using TermBullet.Application.Items;
using TermBullet.Domain.Items;

namespace TermBullet.Tui.Screens;

public sealed class EditItemFormDraft
{
    public required string PublicRef { get; init; }

    public required ItemType Type { get; init; }

    public string Content { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public ItemCollection Collection { get; set; } = ItemCollection.Today;

    public Priority Priority { get; set; } = Priority.None;

    public IReadOnlyCollection<string> SelectedTags { get; set; } = [];

    public string ScheduledAtText { get; set; } = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd");

    public static EditItemFormDraft FromRow(ItemDisplayRow row) =>
        new()
        {
            PublicRef = row.PublicRef,
            Type = ParseType(row.Type),
            Content = row.Content,
            Description = row.Description ?? string.Empty,
            Collection = ParseCollection(row.Collection),
            Priority = ParsePriority(row.Priority),
            SelectedTags = row.Tags,
            ScheduledAtText = row.ScheduledAt is null
                ? DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd")
                : DateOnly.FromDateTime(row.ScheduledAt.Value.UtcDateTime).ToString("yyyy-MM-dd")
        };

    public EditItemRequest BuildRequest()
    {
        var content = NormalizeRequiredText(Content);
        var tags = NormalizeTags(SelectedTags);
        DateTimeOffset? scheduledAt = Type == ItemType.Event ? ToUtcInstant(ParseScheduledAt()) : null;

        return new EditItemRequest
        {
            PublicRef = PublicRef,
            Content = content,
            Description = NormalizeOptionalText(Description),
            Collection = Type == ItemType.Task ? Collection : null,
            Priority = Type == ItemType.Task ? Priority : Priority.None,
            Tags = tags.Count > 0 ? tags : [],
            ScheduledAt = scheduledAt
        };
    }

    public IReadOnlyList<string> BuildPreviewLines()
    {
        return
        [
            $"ref: {PublicRef}",
            $"type: {Type.ToString().ToLowerInvariant()}",
            $"content: {(string.IsNullOrWhiteSpace(Content) ? "(required)" : Content.Trim())}",
            $"description: {(string.IsNullOrWhiteSpace(Description) ? "-" : Description.Trim())}",
            Type == ItemType.Task ? $"collection: {Collection.ToString().ToLowerInvariant()}" : "collection: unchanged",
            Type == ItemType.Task ? $"priority: {Priority.ToString().ToLowerInvariant()}" : "priority: none",
            Type == ItemType.Event ? $"scheduled_at: {ScheduledAtText.Trim()}" : "scheduled_at: -",
            $"tags: {FormatTags(SelectedTags)}"
        ];
    }

    private DateOnly ParseScheduledAt()
    {
        if (!DateOnly.TryParseExact(
            ScheduledAtText.Trim(),
            "yyyy-MM-dd",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None,
            out var scheduledAt))
        {
            throw new ArgumentException("Scheduled at must use yyyy-MM-dd.");
        }

        return scheduledAt;
    }

    private static DateTimeOffset ToUtcInstant(DateOnly date) =>
        new(date.Year, date.Month, date.Day, 0, 0, 0, TimeSpan.Zero);

    private static string NormalizeRequiredText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Content is required.");
        }

        return value.Trim();
    }

    private static string? NormalizeOptionalText(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static List<string> NormalizeTags(IEnumerable<string>? value)
    {
        if (value is null)
        {
            return [];
        }

        return
        [
            .. value
                .Select(tag => tag.Trim())
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Distinct(StringComparer.OrdinalIgnoreCase)
        ];
    }

    private static string FormatTags(IEnumerable<string>? value)
    {
        var tags = NormalizeTags(value);
        return tags.Count > 0 ? string.Join(", ", tags) : "-";
    }

    private static ItemType ParseType(string value) =>
        value.ToLowerInvariant() switch
        {
            "task" => ItemType.Task,
            "note" => ItemType.Note,
            "event" => ItemType.Event,
            _ => ItemType.Task
        };

    private static ItemCollection ParseCollection(string value) =>
        value.ToLowerInvariant() switch
        {
            "week" => ItemCollection.Week,
            "month" => ItemCollection.Month,
            "backlog" => ItemCollection.Backlog,
            _ => ItemCollection.Today
        };

    private static Priority ParsePriority(string value) =>
        value.ToLowerInvariant() switch
        {
            "low" => Priority.Low,
            "medium" => Priority.Medium,
            "high" => Priority.High,
            _ => Priority.None
        };
}
