using TermBullet.Application.Items;
using TermBullet.Domain.Items;
using TermBullet.Tui.Screens;

namespace TermBullet.Tests.Tui;

public sealed class ItemDetailViewModelTests
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 5, 9, 8, 14, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset UpdatedAt = new(2026, 5, 9, 10, 31, 0, TimeSpan.Zero);

    [Fact]
    public void FromItem_builds_task_detail_without_internal_identity_fields()
    {
        var item = MakeItem(
            type: ItemType.Task,
            publicRef: "t-0526-1",
            collection: ItemCollection.Today,
            priority: Priority.High);

        var vm = ItemDetailViewModel.FromItem(item);

        Assert.Equal("t-0526-1", vm.PublicRef);
        Assert.Equal("Task t-0526-1", vm.DetailTitle);
        Assert.Equal("Planning", vm.SummaryTitle);
        Assert.Contains(vm.SummaryLines, line => line.Contains("status: open", StringComparison.Ordinal));
        Assert.Contains(vm.SummaryLines, line => line.Contains("collection: today", StringComparison.Ordinal));
        Assert.Contains(vm.SummaryLines, line => line.Contains("priority: high", StringComparison.Ordinal));
        Assert.Contains(vm.SummaryLines, line => line.Contains("tag: auth", StringComparison.Ordinal));
        Assert.DoesNotContain(vm.SummaryLines, line => line.Contains("scheduled_at", StringComparison.Ordinal));
        Assert.Contains(vm.ContentLines, line => line.Contains("title: Fix auth flow", StringComparison.Ordinal));
        Assert.Contains(vm.ContentLines, line => line.Contains("reproduce login failure", StringComparison.Ordinal));
    }

    [Fact]
    public void FromItem_builds_note_detail_without_collection_or_priority_noise()
    {
        var item = MakeItem(
            type: ItemType.Note,
            publicRef: "n-0526-1",
            collection: ItemCollection.Notes,
            priority: Priority.None);

        var vm = ItemDetailViewModel.FromItem(item);

        Assert.Equal("Note n-0526-1", vm.DetailTitle);
        Assert.Equal("Info", vm.SummaryTitle);
        Assert.Contains(vm.SummaryLines, line => line.Contains("status: open", StringComparison.Ordinal));
        Assert.Contains(vm.SummaryLines, line => line.Contains("tag: auth", StringComparison.Ordinal));
        Assert.Contains(vm.SummaryLines, line => line.Contains("updated: 2026-05-09T10:31:00Z", StringComparison.Ordinal));
        Assert.DoesNotContain(vm.SummaryLines, line => line.Contains("collection", StringComparison.Ordinal));
        Assert.DoesNotContain(vm.SummaryLines, line => line.Contains("priority", StringComparison.Ordinal));
        Assert.DoesNotContain(vm.SummaryLines, line => line.Contains("scheduled", StringComparison.Ordinal));
    }

    [Fact]
    public void FromItem_builds_event_detail_with_schedule()
    {
        var item = MakeItem(
            type: ItemType.Event,
            publicRef: "e-0526-1",
            collection: ItemCollection.Events,
            priority: Priority.None,
            scheduledAt: new DateTimeOffset(2026, 5, 12, 0, 0, 0, TimeSpan.Zero));

        var vm = ItemDetailViewModel.FromItem(item);

        Assert.Equal("Event e-0526-1", vm.DetailTitle);
        Assert.Equal("Schedule", vm.SummaryTitle);
        Assert.Contains(vm.SummaryLines, line => line.Contains("status: open", StringComparison.Ordinal));
        Assert.Contains(vm.SummaryLines, line => line.Contains("scheduled: 2026-05-12", StringComparison.Ordinal));
        Assert.Contains(vm.SummaryLines, line => line.Contains("tag: auth", StringComparison.Ordinal));
        Assert.DoesNotContain(vm.SummaryLines, line => line.Contains("priority", StringComparison.Ordinal));
    }

    [Fact]
    public void History_lines_show_item_history_entries()
    {
        var vm = ItemDetailViewModel.FromItem(
            MakeItem(),
            [
                new ItemHistoryEntryResult(
                    new DateTimeOffset(2026, 5, 9, 10, 45, 0, TimeSpan.Zero),
                    "migrate",
                    "migrate: today -> week")
            ]);

        Assert.Contains(vm.HistoryLines, line => line.Contains("2026-05-09T10:45:00Z migrate: today -> week", StringComparison.Ordinal));
    }

    [Fact]
    public void History_lines_show_empty_state_when_no_history_entries_exist()
    {
        var vm = ItemDetailViewModel.FromItem(MakeItem());

        Assert.Contains(vm.HistoryLines, line => line.Contains("no history entries", StringComparison.OrdinalIgnoreCase));
    }

    private static ItemResult MakeItem(
        ItemType type = ItemType.Task,
        string publicRef = "t-0526-1",
        ItemCollection collection = ItemCollection.Today,
        Priority priority = Priority.High,
        DateTimeOffset? scheduledAt = null) =>
        new(
            Id: Guid.NewGuid(),
            PublicRef: publicRef,
            Type: type,
            Content: "Fix auth flow",
            Description: "reproduce login failure",
            Status: ItemStatus.Open,
            Collection: collection,
            Priority: priority,
            Tag: "auth",
            ScheduledAt: scheduledAt,
            Version: 3,
            CreatedAt: CreatedAt,
            UpdatedAt: UpdatedAt);
}
