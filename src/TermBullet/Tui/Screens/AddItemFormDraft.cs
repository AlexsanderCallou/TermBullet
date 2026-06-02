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

    public string SelectedTag { get; set; } = Item.DefaultTag;

    public string ScheduledAtText { get; set; } = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd");

    public static CreateItemRequest BuildQuickTaskRequest(string content, string? tag = null)
    {
        return new CreateItemRequest
        {
            Type = ItemType.Task,
            Content = NormalizeRequiredText(content, nameof(content)),
            Collection = ItemCollection.Today,
            Tag = NormalizeTagOrDefault(tag)
        };
    }

    public CreateItemRequest BuildRequest()
    {
        var content = NormalizeRequiredText(Content, nameof(Content));
        var description = NormalizeOptionalText(Description);
        var collection = ResolveCollection();
        var scheduledAt = ResolveScheduledDate();

        return new CreateItemRequest
        {
            Type = Type,
            Content = content,
            Collection = collection,
            Description = description,
            Priority = Type == ItemType.Task ? Priority : TermBullet.Domain.Items.Priority.None,
            Tag = NormalizeTagOrDefault(SelectedTag),
            ScheduledAt = Type == ItemType.Event ? ToUtcInstant(scheduledAt!.Value) : null
        };
    }

    public IReadOnlyList<string> BuildPreviewLines()
    {
        var content = string.IsNullOrWhiteSpace(Content) ? "(required)" : Content.Trim();
        var description = string.IsNullOrWhiteSpace(Description) ? "-" : Description.Trim();
        var lines = new List<string>
        {
            $"type: {Type.ToString().ToLowerInvariant()}",
            $"content: {content}",
            $"description: {description}"
        };

        if (Type == ItemType.Task)
        {
            lines.Add($"collection: {ResolveCollection().ToString().ToLowerInvariant()}");
            lines.Add($"priority: {FormatPriority(Priority)}");
        }

        if (Type == ItemType.Event)
        {
            lines.Add($"scheduled_at: {ResolveScheduledDate()!.Value:yyyy-MM-dd}");
        }

        lines.Add($"tag: {NormalizeTagOrDefault(SelectedTag)}");
        return lines;
    }

    private ItemCollection ResolveCollection() =>
        Type switch
        {
            ItemType.Note => ItemCollection.Notes,
            ItemType.Event => ItemCollection.Events,
            _ => Timing switch
            {
                AddItemTimingChoice.Today => ItemCollection.Today,
                AddItemTimingChoice.Week => ItemCollection.Week,
                AddItemTimingChoice.Month => ItemCollection.Month,
                AddItemTimingChoice.Backlog => ItemCollection.Backlog,
                _ => ItemCollection.Today
            }
        };

    private DateOnly ParseScheduledAt()
    {
        var value = ScheduledAtText.Trim();
        if (!DateOnly.TryParseExact(
            value,
            "yyyy-MM-dd",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None,
            out var scheduledAt))
        {
            throw new ArgumentException("Scheduled at must use yyyy-MM-dd.", nameof(ScheduledAtText));
        }

        return scheduledAt;
    }

    private DateOnly? ResolveScheduledDate() =>
        Type switch
        {
            ItemType.Note => null,
            ItemType.Event => ParseScheduledAt(),
            _ => null
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

    private static string NormalizeTagOrDefault(string? value) =>
        string.IsNullOrWhiteSpace(value) ? Item.DefaultTag : value.Trim().ToLowerInvariant();

    private static string FormatPriority(Priority priority) =>
        priority.ToString().ToLowerInvariant();
}
