using TermBullet.Application.Items;
using TermBullet.Core.Items;
using TermBullet.Tui.Screens;

namespace TermBullet.Tests.Tui;

public sealed class DailyFocusViewModelTests
{
    [Fact]
    public void Groups_items_by_status_section()
    {
        var items = new[]
        {
            MakeItem("t-0426-1", ItemStatus.Open, "Fix JWT"),
            MakeItem("t-0426-6", ItemStatus.InProgress, "Work in progress"),
            MakeItem("t-0426-2", ItemStatus.Done, "Deploy prod"),
            MakeItem("t-0426-3", ItemStatus.Cancelled, "Old task"),
            MakeItem("t-0426-4", ItemStatus.Open, "Review PR"),
            MakeItem("t-0426-5", ItemStatus.Migrated, "Moved item")
        };

        var vm = new DailyFocusViewModel(items);

        Assert.Equal(2, vm.OpenItems.Count);
        Assert.Single(vm.InProgressItems);
        Assert.Single(vm.DoneItems);
        Assert.Single(vm.CancelledItems);
        Assert.Single(vm.MigratedItems);
    }

    [Fact]
    public void ActiveSection_starts_at_Open()
    {
        var vm = new DailyFocusViewModel([]);

        Assert.Equal(DailyFocusSection.Open, vm.ActiveSection);
    }

    [Fact]
    public void SelectedItemIndex_starts_at_zero_when_section_has_items()
    {
        var items = new[] { MakeItem("t-0426-1", ItemStatus.Open, "Fix JWT") };
        var vm = new DailyFocusViewModel(items);

        Assert.Equal(0, vm.SelectedItemIndex);
    }

    [Fact]
    public void SelectedItemIndex_is_minus_one_when_section_is_empty()
    {
        var vm = new DailyFocusViewModel([]);

        Assert.Equal(-1, vm.SelectedItemIndex);
    }

    [Fact]
    public void SelectNextItem_advances_selection_in_active_section()
    {
        var items = new[]
        {
            MakeItem("t-0426-1", ItemStatus.Open, "First"),
            MakeItem("t-0426-2", ItemStatus.Open, "Second")
        };
        var vm = new DailyFocusViewModel(items);

        vm.SelectNextItem();

        Assert.Equal(1, vm.SelectedItemIndex);
    }

    [Fact]
    public void SelectPreviousItem_moves_selection_back()
    {
        var items = new[]
        {
            MakeItem("t-0426-1", ItemStatus.Open, "First"),
            MakeItem("t-0426-2", ItemStatus.Open, "Second")
        };
        var vm = new DailyFocusViewModel(items);
        vm.SelectNextItem();

        vm.SelectPreviousItem();

        Assert.Equal(0, vm.SelectedItemIndex);
    }

    [Fact]
    public void ChangeSection_switches_active_section_and_resets_selection()
    {
        var items = new[]
        {
            MakeItem("t-0426-1", ItemStatus.Open, "Fix JWT"),
            MakeItem("t-0426-2", ItemStatus.Done, "Deploy prod")
        };
        var vm = new DailyFocusViewModel(items);
        vm.SelectNextItem();

        vm.ChangeSection(DailyFocusSection.Done);

        Assert.Equal(DailyFocusSection.Done, vm.ActiveSection);
        Assert.Equal(0, vm.SelectedItemIndex);
    }

    [Fact]
    public void SelectedItem_returns_item_from_active_section_at_selected_index()
    {
        var items = new[]
        {
            MakeItem("t-0426-1", ItemStatus.Open, "First"),
            MakeItem("t-0426-2", ItemStatus.Open, "Second")
        };
        var vm = new DailyFocusViewModel(items);
        vm.SelectNextItem();

        Assert.Equal("t-0426-2", vm.SelectedItem?.PublicRef);
    }

    [Fact]
    public void SelectedItem_is_null_when_active_section_is_empty()
    {
        var vm = new DailyFocusViewModel([]);

        Assert.Null(vm.SelectedItem);
    }

    [Fact]
    public void ActiveSectionItems_returns_items_for_active_section()
    {
        var items = new[]
        {
            MakeItem("t-0426-1", ItemStatus.Open, "Fix JWT"),
            MakeItem("t-0426-9", ItemStatus.InProgress, "Review PR"),
            MakeItem("t-0426-2", ItemStatus.Done, "Deploy prod")
        };
        var vm = new DailyFocusViewModel(items);

        Assert.Single(vm.ActiveSectionItems);
        Assert.Equal("t-0426-1", vm.ActiveSectionItems[0].PublicRef);

        vm.ChangeSection(DailyFocusSection.InProgress);

        Assert.Single(vm.ActiveSectionItems);
        Assert.Equal("t-0426-9", vm.ActiveSectionItems[0].PublicRef);

        vm.ChangeSection(DailyFocusSection.Done);

        Assert.Single(vm.ActiveSectionItems);
        Assert.Equal("t-0426-2", vm.ActiveSectionItems[0].PublicRef);
    }

    private static ItemResult MakeItem(string publicRef, ItemStatus status, string content) =>
        new(
            Id: Guid.NewGuid(),
            PublicRef: publicRef,
            Type: ItemType.Task,
            Content: content,
            Description: null,
            Status: status,
            Collection: ItemCollection.Today,
            Priority: Priority.None,
            Tags: [],
            Version: 1,
            CreatedAt: DateTimeOffset.UtcNow,
            UpdatedAt: DateTimeOffset.UtcNow);
}
