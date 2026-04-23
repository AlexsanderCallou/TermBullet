using TermBullet.Core.Items;
using TermBullet.Core.Refs;

namespace TermBullet.Tests.Core.Items;

public sealed class ItemTests
{
    private static readonly Guid ItemId = Guid.Parse("0f3a9d94-4df0-47f7-95c1-0f967c22f4db");
    private static readonly DateTimeOffset CreatedAt = new(2026, 4, 23, 10, 30, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset ChangedAt = new(2026, 4, 23, 11, 45, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(ItemType.Task, "t-0426-1")]
    [InlineData(ItemType.Note, "n-0426-1")]
    [InlineData(ItemType.Event, "e-0426-1")]
    public void Create_returns_open_item_with_defaults(ItemType itemType, string publicRefValue)
    {
        var publicRef = PublicRef.Parse(publicRefValue);

        var item = Item.Create(
            ItemId,
            publicRef,
            itemType,
            "  Fix authentication flow  ",
            ItemCollection.Today,
            CreatedAt,
            description: "  Keep CLI and TUI behavior aligned.  ",
            tags: ["auth", "cli"]);

        Assert.Equal(ItemId, item.Id);
        Assert.Same(publicRef, item.PublicRef);
        Assert.Equal(itemType, item.Type);
        Assert.Equal("Fix authentication flow", item.Content);
        Assert.Equal("Keep CLI and TUI behavior aligned.", item.Description);
        Assert.Equal(ItemStatus.Open, item.Status);
        Assert.Equal(ItemCollection.Today, item.Collection);
        Assert.Equal(Priority.None, item.Priority);
        Assert.Equal(["auth", "cli"], item.Tags);
        Assert.Equal(1, item.Version);
        Assert.Equal(CreatedAt, item.CreatedAt);
        Assert.Equal(CreatedAt, item.UpdatedAt);
        Assert.Null(item.DueAt);
        Assert.Null(item.ScheduledAt);
        Assert.Null(item.EstimateMinutes);
        Assert.Null(item.CompletedAt);
        Assert.Null(item.CancelledAt);
        Assert.Null(item.MigratedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Create_rejects_empty_content(string content)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => CreateTask(content: content));

        Assert.Equal("content", exception.ParamName);
    }

    [Fact]
    public void Create_rejects_empty_id()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => CreateTask(id: Guid.Empty));

        Assert.Equal("id", exception.ParamName);
    }

    [Fact]
    public void Create_rejects_null_public_ref()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => Item.Create(
                ItemId,
                publicRef: null!,
                ItemType.Task,
                "Fix authentication flow",
                ItemCollection.Today,
                CreatedAt));

        Assert.Equal("publicRef", exception.ParamName);
    }

    [Fact]
    public void Create_rejects_invalid_item_type()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateTask(itemType: (ItemType)99));

        Assert.Equal("type", exception.ParamName);
    }

    [Fact]
    public void Create_rejects_public_ref_with_different_item_type()
    {
        var noteRef = PublicRef.Parse("n-0426-1");

        var exception = Assert.Throws<ArgumentException>(
            () => Item.Create(
                ItemId,
                noteRef,
                ItemType.Task,
                "Fix authentication flow",
                ItemCollection.Today,
                CreatedAt));

        Assert.Equal("publicRef", exception.ParamName);
    }

    [Fact]
    public void Create_rejects_invalid_collection()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateTask(collection: (ItemCollection)99));

        Assert.Equal("collection", exception.ParamName);
    }

    [Fact]
    public void Create_rejects_invalid_priority()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateTask(priority: (Priority)99));

        Assert.Equal("priority", exception.ParamName);
    }

    [Fact]
    public void Mark_in_progress_sets_status_and_increments_version()
    {
        var item = CreateTask();

        item.MarkInProgress(ChangedAt);

        Assert.Equal(ItemStatus.InProgress, item.Status);
        Assert.Equal(2, item.Version);
        Assert.Equal(ChangedAt, item.UpdatedAt);
        Assert.Null(item.CompletedAt);
    }

    [Fact]
    public void Mark_done_sets_completion_timestamp_and_increments_version()
    {
        var item = CreateTask();

        item.MarkDone(ChangedAt);

        Assert.Equal(ItemStatus.Done, item.Status);
        Assert.Equal(2, item.Version);
        Assert.Equal(ChangedAt, item.UpdatedAt);
        Assert.Equal(ChangedAt, item.CompletedAt);
        Assert.Null(item.CancelledAt);
        Assert.Null(item.MigratedAt);
    }

    [Fact]
    public void Cancel_sets_cancel_timestamp_and_increments_version()
    {
        var item = CreateTask();

        item.Cancel(ChangedAt);

        Assert.Equal(ItemStatus.Cancelled, item.Status);
        Assert.Equal(2, item.Version);
        Assert.Equal(ChangedAt, item.UpdatedAt);
        Assert.Equal(ChangedAt, item.CancelledAt);
        Assert.Null(item.CompletedAt);
        Assert.Null(item.MigratedAt);
    }

    [Fact]
    public void Mark_migrated_sets_migration_timestamp_and_increments_version()
    {
        var item = CreateTask();

        item.MarkMigrated(ChangedAt);

        Assert.Equal(ItemStatus.Migrated, item.Status);
        Assert.Equal(2, item.Version);
        Assert.Equal(ChangedAt, item.UpdatedAt);
        Assert.Equal(ChangedAt, item.MigratedAt);
        Assert.Null(item.CompletedAt);
        Assert.Null(item.CancelledAt);
    }

    [Theory]
    [InlineData(ItemStatus.Done)]
    [InlineData(ItemStatus.Cancelled)]
    [InlineData(ItemStatus.Migrated)]
    public void Terminal_statuses_reject_further_status_changes(ItemStatus terminalStatus)
    {
        var item = CreateTask();
        MoveToTerminalStatus(item, terminalStatus);

        var exception = Assert.Throws<InvalidOperationException>(
            () => item.MarkInProgress(ChangedAt.AddMinutes(5)));

        Assert.Contains("terminal", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Set_priority_updates_priority_and_increments_version()
    {
        var item = CreateTask();

        item.SetPriority(Priority.High, ChangedAt);

        Assert.Equal(Priority.High, item.Priority);
        Assert.Equal(2, item.Version);
        Assert.Equal(ChangedAt, item.UpdatedAt);
    }

    [Fact]
    public void Set_priority_rejects_invalid_priority()
    {
        var item = CreateTask();

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => item.SetPriority((Priority)99, ChangedAt));

        Assert.Equal("priority", exception.ParamName);
    }

    [Fact]
    public void Move_to_collection_updates_collection_and_increments_version()
    {
        var item = CreateTask();

        item.MoveTo(ItemCollection.Backlog, ChangedAt);

        Assert.Equal(ItemCollection.Backlog, item.Collection);
        Assert.Equal(2, item.Version);
        Assert.Equal(ChangedAt, item.UpdatedAt);
    }

    [Fact]
    public void Move_to_collection_rejects_invalid_collection()
    {
        var item = CreateTask();

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => item.MoveTo((ItemCollection)99, ChangedAt));

        Assert.Equal("collection", exception.ParamName);
    }

    private static Item CreateTask(
        Guid? id = null,
        ItemType itemType = ItemType.Task,
        string content = "Fix authentication flow",
        ItemCollection collection = ItemCollection.Today,
        Priority priority = Priority.None)
    {
        return Item.Create(
            id ?? ItemId,
            PublicRef.Parse("t-0426-1"),
            itemType,
            content,
            collection,
            CreatedAt,
            priority: priority);
    }

    private static void MoveToTerminalStatus(Item item, ItemStatus status)
    {
        switch (status)
        {
            case ItemStatus.Done:
                item.MarkDone(ChangedAt);
                break;
            case ItemStatus.Cancelled:
                item.Cancel(ChangedAt);
                break;
            case ItemStatus.Migrated:
                item.MarkMigrated(ChangedAt);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, "Status is not terminal.");
        }
    }
}
