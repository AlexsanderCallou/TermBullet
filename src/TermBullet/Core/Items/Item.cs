using TermBullet.Core.Refs;

namespace TermBullet.Core.Items;

public sealed class Item
{
    private readonly List<string> _tags;

    private Item(
        Guid id,
        PublicRef publicRef,
        ItemType type,
        string content,
        string? description,
        ItemCollection collection,
        Priority priority,
        IReadOnlyCollection<string> tags,
        DateTimeOffset createdAt,
        DateTimeOffset? dueAt,
        DateTimeOffset? scheduledAt,
        int? estimateMinutes)
    {
        Id = id;
        PublicRef = publicRef;
        Type = type;
        Content = content;
        Description = description;
        Status = ItemStatus.Open;
        Collection = collection;
        Priority = priority;
        _tags = [.. tags];
        Version = 1;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
        DueAt = dueAt;
        ScheduledAt = scheduledAt;
        EstimateMinutes = estimateMinutes;
    }

    private Item(
        Guid id,
        PublicRef publicRef,
        ItemType type,
        string content,
        string? description,
        ItemStatus status,
        ItemCollection collection,
        Priority priority,
        IReadOnlyCollection<string> tags,
        int version,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        DateTimeOffset? dueAt,
        DateTimeOffset? scheduledAt,
        int? estimateMinutes,
        DateTimeOffset? completedAt,
        DateTimeOffset? cancelledAt,
        DateTimeOffset? migratedAt,
        MigrationInfo? migration)
    {
        Id = id;
        PublicRef = publicRef;
        Type = type;
        Content = content;
        Description = description;
        Status = status;
        Collection = collection;
        Priority = priority;
        _tags = [.. tags];
        Version = version;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        DueAt = dueAt;
        ScheduledAt = scheduledAt;
        EstimateMinutes = estimateMinutes;
        CompletedAt = completedAt;
        CancelledAt = cancelledAt;
        MigratedAt = migratedAt;
        Migration = migration;
    }

    public Guid Id { get; }

    public PublicRef PublicRef { get; }

    public ItemType Type { get; }

    public string Content { get; private set; }

    public string? Description { get; private set; }

    public ItemStatus Status { get; private set; }

    public ItemCollection Collection { get; private set; }

    public Priority Priority { get; private set; }

    public IReadOnlyList<string> Tags => _tags.AsReadOnly();

    public int Version { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public DateTimeOffset? DueAt { get; private set; }

    public DateTimeOffset? ScheduledAt { get; private set; }

    public int? EstimateMinutes { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public DateTimeOffset? CancelledAt { get; private set; }

    public DateTimeOffset? MigratedAt { get; private set; }

    public MigrationInfo? Migration { get; private set; }

    public static Item Create(
        Guid id,
        PublicRef publicRef,
        ItemType type,
        string content,
        ItemCollection collection,
        DateTimeOffset createdAt,
        string? description = null,
        Priority priority = Priority.None,
        IEnumerable<string>? tags = null,
        DateTimeOffset? dueAt = null,
        DateTimeOffset? scheduledAt = null,
        int? estimateMinutes = null)
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

        EnsureDefined(collection, nameof(collection));
        EnsureDefined(priority, nameof(priority));

        if (estimateMinutes is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(estimateMinutes), "Estimate minutes must not be negative.");
        }

        return new Item(
            id,
            publicRef,
            type,
            normalizedContent,
            normalizedDescription,
            collection,
            priority,
            NormalizeTags(tags),
            createdAt,
            dueAt,
            scheduledAt,
            estimateMinutes);
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
        IEnumerable<string>? tags,
        int version,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        DateTimeOffset? dueAt = null,
        DateTimeOffset? scheduledAt = null,
        int? estimateMinutes = null,
        DateTimeOffset? completedAt = null,
        DateTimeOffset? cancelledAt = null,
        DateTimeOffset? migratedAt = null,
        MigrationInfo? migration = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Item ID must not be empty.", nameof(id));
        }

        ArgumentNullException.ThrowIfNull(publicRef);
        EnsureDefined(type, nameof(type));
        EnsureDefined(status, nameof(status));
        EnsureDefined(collection, nameof(collection));
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

        if (estimateMinutes is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(estimateMinutes), "Estimate minutes must not be negative.");
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
            NormalizeTags(tags),
            version,
            createdAt,
            updatedAt,
            dueAt,
            scheduledAt,
            estimateMinutes,
            completedAt,
            cancelledAt,
            migratedAt,
            migration);
    }

    public void MarkInProgress(DateTimeOffset changedAt)
    {
        EnsureActive();
        Status = ItemStatus.InProgress;
        Touch(changedAt);
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

    public void MarkMigrated(DateTimeOffset changedAt)
    {
        EnsureActive();
        Status = ItemStatus.Migrated;
        MigratedAt = changedAt;
        Touch(changedAt);
    }

    public void ApplyMigrationInfo(MigrationInfo migration, DateTimeOffset changedAt)
    {
        ArgumentNullException.ThrowIfNull(migration);
        Migration = migration;
        MigratedAt = migration.MigratedAt;
        Touch(changedAt);
    }

    public void SetPriority(Priority priority, DateTimeOffset changedAt)
    {
        EnsureActive();
        EnsureDefined(priority, nameof(priority));
        Priority = priority;
        Touch(changedAt);
    }

    public void MoveTo(ItemCollection collection, DateTimeOffset changedAt)
    {
        EnsureActive();
        EnsureDefined(collection, nameof(collection));
        Collection = collection;
        Touch(changedAt);
    }

    public void Edit(string content, DateTimeOffset changedAt, string? description = null)
    {
        EnsureActive();
        Content = NormalizeRequiredText(content, nameof(content));
        Description = NormalizeOptionalText(description);
        Touch(changedAt);
    }

    public void AddTag(string tag, DateTimeOffset changedAt)
    {
        EnsureActive();
        var normalizedTag = NormalizeRequiredText(tag, nameof(tag));
        if (_tags.Contains(normalizedTag, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        _tags.Add(normalizedTag);
        Touch(changedAt);
    }

    public void RemoveTag(string tag, DateTimeOffset changedAt)
    {
        EnsureActive();
        var normalizedTag = NormalizeRequiredText(tag, nameof(tag));
        var index = _tags.FindIndex(existingTag => string.Equals(
            existingTag,
            normalizedTag,
            StringComparison.OrdinalIgnoreCase));

        if (index < 0)
        {
            return;
        }

        _tags.RemoveAt(index);
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

    private static IReadOnlyCollection<string> NormalizeTags(IEnumerable<string>? tags)
    {
        if (tags is null)
        {
            return [];
        }

        var normalizedTags = new List<string>();
        foreach (var tag in tags)
        {
            var normalizedTag = NormalizeRequiredText(tag, nameof(tags));
            if (!normalizedTags.Contains(normalizedTag, StringComparer.OrdinalIgnoreCase))
            {
                normalizedTags.Add(normalizedTag);
            }
        }

        return normalizedTags;
    }

    private void EnsureActive()
    {
        if (Status is ItemStatus.Done or ItemStatus.Cancelled or ItemStatus.Migrated)
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
