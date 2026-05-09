using TermBullet.Application.Items;
using TermBullet.Core.Items;
using TermBullet.Tui.Screens;

namespace TermBullet.Tests.Tui;

public sealed class ItemDetailViewModelTests
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 5, 9, 8, 14, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset UpdatedAt = new(2026, 5, 9, 10, 31, 0, TimeSpan.Zero);

    [Fact]
    public void FromItem_maps_all_available_item_fields()
    {
        var item = MakeItem();

        var vm = ItemDetailViewModel.FromItem(item);

        Assert.Equal("t-0526-1", vm.PublicRef);
        Assert.Equal("Fix auth flow", vm.Content);
        Assert.Contains(vm.IdentityLines, line => line.Contains(item.Id.ToString(), StringComparison.Ordinal));
        Assert.Contains(vm.IdentityLines, line => line.Contains("type: task", StringComparison.Ordinal));
        Assert.Contains(vm.IdentityLines, line => line.Contains("version: 3", StringComparison.Ordinal));
        Assert.Contains(vm.PlanningLines, line => line.Contains("collection: today", StringComparison.Ordinal));
        Assert.Contains(vm.PlanningLines, line => line.Contains("planned_for: 2026-05-09", StringComparison.Ordinal));
        Assert.Contains(vm.PlanningLines, line => line.Contains("priority: high", StringComparison.Ordinal));
        Assert.Contains(vm.ContentLines, line => line.Contains("Fix auth flow", StringComparison.Ordinal));
        Assert.Contains(vm.ContentLines, line => line.Contains("reproduce login failure", StringComparison.Ordinal));
    }

    [Fact]
    public void History_lines_explain_current_limitation_when_history_is_not_loaded()
    {
        var vm = ItemDetailViewModel.FromItem(MakeItem());

        Assert.Contains(vm.HistoryLines, line => line.Contains("history not loaded", StringComparison.OrdinalIgnoreCase));
    }

    private static ItemResult MakeItem() =>
        new(
            Id: Guid.NewGuid(),
            PublicRef: "t-0526-1",
            Type: ItemType.Task,
            Content: "Fix auth flow",
            Description: "reproduce login failure",
            Status: ItemStatus.Open,
            Collection: ItemCollection.Today,
            Priority: Priority.High,
            Tags: ["auth", "cli"],
            PlannedFor: new DateOnly(2026, 5, 9),
            ScheduledAt: null,
            Version: 3,
            CreatedAt: CreatedAt,
            UpdatedAt: UpdatedAt);
}
