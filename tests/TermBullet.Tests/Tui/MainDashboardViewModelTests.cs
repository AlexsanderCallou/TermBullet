using TermBullet.Application.Items;
using TermBullet.Core.Items;
using TermBullet.Tui.Screens;

namespace TermBullet.Tests.Tui;

public sealed class MainDashboardViewModelTests
{
    [Fact]
    public void DayItems_maps_today_results_to_display_rows()
    {
        var items = new[]
        {
            MakeItem("t-0426-1", ItemType.Task, ItemStatus.Open, "Fix auth JWT", Priority.High),
            MakeItem("n-0426-1", ItemType.Note, ItemStatus.Open, "Investigate error", Priority.None)
        };

        var vm = new MainDashboardViewModel(dayItems: items, backlogItems: []);

        Assert.Equal(2, vm.DayItems.Count);
        Assert.Equal("t-0426-1", vm.DayItems[0].PublicRef);
        Assert.Equal("[ ]", vm.DayItems[0].Symbol);
        Assert.Equal("Fix auth JWT", vm.DayItems[0].Content);
        Assert.Equal("high", vm.DayItems[0].Priority);

        Assert.Equal("n-0426-1", vm.DayItems[1].PublicRef);
        Assert.Equal("(.)", vm.DayItems[1].Symbol);
    }

    [Fact]
    public void DayItems_is_empty_when_no_today_items()
    {
        var vm = new MainDashboardViewModel(dayItems: [], backlogItems: []);

        Assert.Empty(vm.DayItems);
    }

    [Fact]
    public void BacklogItems_maps_backlog_results_to_display_rows()
    {
        var items = new[]
        {
            MakeItem("t-0426-3", ItemType.Task, ItemStatus.Open, "Adjust compose", Priority.Medium)
        };

        var vm = new MainDashboardViewModel(dayItems: [], backlogItems: items);

        Assert.Single(vm.BacklogItems);
        Assert.Equal("t-0426-3", vm.BacklogItems[0].PublicRef);
        Assert.Equal("Adjust compose", vm.BacklogItems[0].Content);
    }

    [Fact]
    public void SelectedDayItemIndex_starts_at_zero_when_there_are_items()
    {
        var items = new[] { MakeItem("t-0426-1", ItemType.Task, ItemStatus.Open, "Fix auth JWT", Priority.None) };
        var vm = new MainDashboardViewModel(dayItems: items, backlogItems: []);

        Assert.Equal(0, vm.SelectedDayItemIndex);
    }

    [Fact]
    public void SelectedDayItemIndex_is_minus_one_when_no_items()
    {
        var vm = new MainDashboardViewModel(dayItems: [], backlogItems: []);

        Assert.Equal(-1, vm.SelectedDayItemIndex);
    }

    [Fact]
    public void SelectNextDayItem_advances_selected_index()
    {
        var items = new[]
        {
            MakeItem("t-0426-1", ItemType.Task, ItemStatus.Open, "First", Priority.None),
            MakeItem("t-0426-2", ItemType.Task, ItemStatus.Open, "Second", Priority.None)
        };
        var vm = new MainDashboardViewModel(dayItems: items, backlogItems: []);

        vm.SelectNextDayItem();

        Assert.Equal(1, vm.SelectedDayItemIndex);
    }

    [Fact]
    public void SelectNextDayItem_does_not_advance_past_last_item()
    {
        var items = new[]
        {
            MakeItem("t-0426-1", ItemType.Task, ItemStatus.Open, "Only", Priority.None)
        };
        var vm = new MainDashboardViewModel(dayItems: items, backlogItems: []);

        vm.SelectNextDayItem();

        Assert.Equal(0, vm.SelectedDayItemIndex);
    }

    [Fact]
    public void SelectPreviousDayItem_moves_selection_back()
    {
        var items = new[]
        {
            MakeItem("t-0426-1", ItemType.Task, ItemStatus.Open, "First", Priority.None),
            MakeItem("t-0426-2", ItemType.Task, ItemStatus.Open, "Second", Priority.None)
        };
        var vm = new MainDashboardViewModel(dayItems: items, backlogItems: []);
        vm.SelectNextDayItem();

        vm.SelectPreviousDayItem();

        Assert.Equal(0, vm.SelectedDayItemIndex);
    }

    [Fact]
    public void SelectedDayItem_returns_item_at_selected_index()
    {
        var items = new[]
        {
            MakeItem("t-0426-1", ItemType.Task, ItemStatus.Open, "First", Priority.None),
            MakeItem("t-0426-2", ItemType.Task, ItemStatus.Open, "Second", Priority.None)
        };
        var vm = new MainDashboardViewModel(dayItems: items, backlogItems: []);
        vm.SelectNextDayItem();

        Assert.Equal("t-0426-2", vm.SelectedDayItem?.PublicRef);
    }

    [Fact]
    public void SelectedDayItem_is_null_when_no_items()
    {
        var vm = new MainDashboardViewModel(dayItems: [], backlogItems: []);

        Assert.Null(vm.SelectedDayItem);
    }

    [Theory]
    [InlineData(ItemType.Task, ItemStatus.Open, "[ ]")]
    [InlineData(ItemType.Task, ItemStatus.Done, "[x]")]
    [InlineData(ItemType.Task, ItemStatus.Cancelled, "[-]")]
    [InlineData(ItemType.Task, ItemStatus.Migrated, "[>]")]
    [InlineData(ItemType.Task, ItemStatus.InProgress, "[~]")]
    [InlineData(ItemType.Note, ItemStatus.Open, "(.)")]
    [InlineData(ItemType.Event, ItemStatus.Open, "(o)")]
    public void Symbol_reflects_type_and_status(ItemType type, ItemStatus status, string expectedSymbol)
    {
        var item = MakeItem("t-0426-1", type, status, "content", Priority.None);
        var vm = new MainDashboardViewModel(dayItems: [item], backlogItems: []);

        Assert.Equal(expectedSymbol, vm.DayItems[0].Symbol);
    }

    private static ItemResult MakeItem(
        string publicRef,
        ItemType type,
        ItemStatus status,
        string content,
        Priority priority) =>
        new(
            Id: Guid.NewGuid(),
            PublicRef: publicRef,
            Type: type,
            Content: content,
            Description: null,
            Status: status,
            Collection: ItemCollection.Today,
            Priority: priority,
            Tags: [],
            Version: 1,
            CreatedAt: DateTimeOffset.UtcNow,
            UpdatedAt: DateTimeOffset.UtcNow);
}
