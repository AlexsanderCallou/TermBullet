using TermBullet.Domain.Refs;

namespace TermBullet.Domain.Items;

public sealed class Item
{
    public const string DefaultTag = "default";

    private Item(
        Guid id,
        PublicRef publicRef,
        ItemType type,
        string content,
        string? description,
        ItemStatus status,
        ItemCollection collection,
        Priority priority,
        string tag,
        int version,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        DateTimeOffset? scheduledAt,
        DateTimeOffset? completedAt,
        DateTimeOffset? cancelledAt)
    {
        Id = id;
        PublicRef = publicRef;
        Type = type;
        Content = content;
        Description = description;
        Status = status;
        Collection = collection;
        Priority = priority;
        Tag = tag;
        Version = version;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        ScheduledAt = scheduledAt;
        CompletedAt = completedAt;
        CancelledAt = cancelledAt;
    }

    public Guid Id { get; }

    public PublicRef PublicRef { get; }

    public ItemType Type { get; }

    public string Content { get; private set; }

    public string? Description { get; private set; }

    public ItemStatus Status { get; private set; }

    public ItemCollection Collection { get; private set; }

    public Priority Priority { get; private set; }

    public string Tag { get; private set; }

    public int Version { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public DateTimeOffset? ScheduledAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public DateTimeOffset? CancelledAt { get; private set; }

    public static Item Create(
        Guid id,
        PublicRef publicRef,
        ItemType type,
        string content,
        ItemCollection collection,
        DateTimeOffset createdAt,
        string? description = null,
        Priority priority = Priority.None,
        string? tag = null,
        DateTimeOffset? scheduledAt = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Item ID must not be empty.", nameof(id));
        }

        ArgumentNullException.ThrowIfNull(publicRef);
        EnsureDefined(type, nameof(type));

        if (publicRef.Type != type)
        {
            throw new ArgumentException("Public ref type must match item type.", nameof(publicRef));
        }

        var normalizedContent = NormalizeRequiredText(content, nameof(content));
        var normalizedDescription = NormalizeOptionalText(description);

        collection = NormalizeCollectionForType(type, collection, nameof(collection));
        EnsureDefined(priority, nameof(priority));

        return new Item(
            id,
            publicRef,
            type,
            normalizedContent,
            normalizedDescription,
            ItemStatus.Open,
            collection,
            priority,
            NormalizeTagOrDefault(tag),
            version: 1,
            createdAt,
            updatedAt: createdAt,
            scheduledAt,
            completedAt: null,
            cancelledAt: null);
    }

    public static Item Restore(
        Guid id,
        PublicRef publicRef,
        ItemType type,
        string content,
        string? description,
        ItemStatus status,
        ItemCollection collection,
        Priority priority,
        string tag,
        int version,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        DateTimeOffset? scheduledAt = null,
        DateTimeOffset? completedAt = null,
        DateTimeOffset? cancelledAt = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Item ID must not be empty.", nameof(id));
        }

        ArgumentNullException.ThrowIfNull(publicRef);
        EnsureDefined(type, nameof(type));
        EnsureDefined(status, nameof(status));
        collection = NormalizeCollectionForType(type, collection, nameof(collection));
        EnsureDefined(priority, nameof(priority));

        if (publicRef.Type != type)
        {
            throw new ArgumentException("Public ref type must match item type.", nameof(publicRef));
        }

        if (version < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(version), "Version must be greater than zero.");
        }

        if (updatedAt < createdAt)
        {
            throw new ArgumentOutOfRangeException(nameof(updatedAt), "UpdatedAt cannot be before CreatedAt.");
        }

        return new Item(
            id,
            publicRef,
            type,
            NormalizeRequiredText(content, nameof(content)),
            NormalizeOptionalText(description),
            status,
            collection,
            priority,
            NormalizeTag(tag, nameof(tag)),
            version,
            createdAt,
            updatedAt,
            scheduledAt,
            completedAt,
            cancelledAt);
    }

    public void MarkDone(DateTimeOffset changedAt)
    {
        EnsureActive();
        Status = ItemStatus.Done;
        CompletedAt = changedAt;
        Touch(changedAt);
    }

    public void Cancel(DateTimeOffset changedAt)
    {
        EnsureActive();
        Status = ItemStatus.Cancelled;
        CancelledAt = changedAt;
        Touch(changedAt);
    }

    public void SetPriority(Priority priority, DateTimeOffset changedAt)
    {
        EnsureActive();
        if (Type != ItemType.Task)
        {
            throw new InvalidOperationException("Priority can only be changed for tasks.");
        }

        EnsureDefined(priority, nameof(priority));
        Priority = priority;
        Touch(changedAt);
    }

    public void MoveTo(ItemCollection collection, DateTimeOffset changedAt)
    {
        EnsureActive();
        if (Type != ItemType.Task)
        {
            throw new InvalidOperationException("Only tasks can be migrated.");
        }

        EnsureTaskCollection(collection, nameof(collection));
        Collection = collection;
        Touch(changedAt);
    }

    public void Edit(
        string content,
        DateTimeOffset changedAt,
        string? description = null,
        ItemCollection? collection = null,
        Priority? priority = null,
        string? tag = null,
        DateTimeOffset? scheduledAt = null)
    {
        EnsureActive();
        Content = NormalizeRequiredText(content, nameof(content));
        Description = NormalizeOptionalText(description);
        if (Type == ItemType.Task)
        {
            if (collection is not null)
            {
                EnsureTaskCollection(collection.Value, nameof(collection));
                Collection = collection.Value;
            }

            if (priority is not null)
            {
                EnsureDefined(priority.Value, nameof(priority));
                Priority = priority.Value;
            }

            ScheduledAt = null;
        }
        else if (Type == ItemType.Event)
        {
            Priority = Priority.None;
            if (scheduledAt is not null)
            {
                ScheduledAt = scheduledAt;
            }
        }
        else
        {
            Priority = Priority.None;
            ScheduledAt = null;
        }

        if (tag is not null)
        {
            Tag = NormalizeTag(tag, nameof(tag));
        }

        Touch(changedAt);
    }

    public void SetTag(string tag, DateTimeOffset changedAt)
    {
        EnsureActive();
        var normalizedTag = NormalizeTag(tag, nameof(tag));
        if (string.Equals(Tag, normalizedTag, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        Tag = normalizedTag;
        Touch(changedAt);
    }

    public void RemoveTag(string tag, DateTimeOffset changedAt)
    {
        EnsureActive();
        var normalizedTag = NormalizeTag(tag, nameof(tag));
        if (!string.Equals(Tag, normalizedTag, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        Tag = DefaultTag;
        Touch(changedAt);
    }

    private static void EnsureDefined<TEnum>(TEnum value, string parameterName)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, $"Unsupported {typeof(TEnum).Name} value.");
        }
    }

    private static ItemCollection NormalizeCollectionForType(
        ItemType type,
        ItemCollection collection,
        string parameterName)
    {
        EnsureDefined(collection, parameterName);

        return type switch
        {
            ItemType.Task => EnsureTaskCollection(collection, parameterName),
            ItemType.Note => ItemCollection.Notes,
            ItemType.Event => ItemCollection.Events,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported item type.")
        };
    }

    private static ItemCollection EnsureTaskCollection(ItemCollection collection, string parameterName)
    {
        EnsureDefined(collection, parameterName);
        return collection switch
        {
            ItemCollection.Today or ItemCollection.Week or ItemCollection.Month or ItemCollection.Backlog => collection,
            _ => throw new ArgumentException("Tasks support only today, week, month, or backlog collections.", parameterName)
        };
    }

    private static string NormalizeRequiredText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value must not be empty.", parameterName);
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

    private static string NormalizeTagOrDefault(string? tag)
    {
        return string.IsNullOrWhiteSpace(tag)
            ? DefaultTag
            : NormalizeTag(tag, nameof(tag));
    }

    private static string NormalizeTag(string tag, string parameterName) =>
        NormalizeRequiredText(tag, parameterName).ToLowerInvariant();

    private void EnsureActive()
    {
        if (Status is ItemStatus.Done or ItemStatus.Cancelled)
        {
            throw new InvalidOperationException($"Item '{PublicRef}' is in a terminal status and cannot be changed.");
        }
    }

    private void Touch(DateTimeOffset changedAt)
    {
        Version++;
        UpdatedAt = changedAt;
    }
}
