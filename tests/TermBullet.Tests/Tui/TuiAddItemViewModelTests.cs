using TermBullet.Domain.Items;
using TermBullet.Tui.Screens;

namespace TermBullet.Tests.Tui;

public sealed class TuiAddItemViewModelTests
{
    [Fact]
    public void ForMainDashboard_UsesTodayAsDefaultCollection()
    {
        var viewModel = TuiAddItemViewModel.ForMainDashboard();

        Assert.Equal(ItemCollection.Today, viewModel.Collection);
    }

    [Fact]
    public void ForType_ProvidesTypeSpecificExamples()
    {
        var task = TuiAddItemViewModel.ForType(ItemType.Task);
        var note = TuiAddItemViewModel.ForType(ItemType.Note);
        var eventVm = TuiAddItemViewModel.ForType(ItemType.Event);

        Assert.Contains(task.Examples, line => line.Contains("review pull request", StringComparison.Ordinal));
        Assert.Contains(note.Examples, line => line.Contains("investigate stacktrace", StringComparison.Ordinal));
        Assert.Contains(eventVm.Examples, line => line.Contains("team sync", StringComparison.Ordinal));
    }

    [Fact]
    public void WithError_PreservesSourceCollectionAndExposesError()
    {
        var viewModel = TuiAddItemViewModel
            .ForMainDashboard()
            .WithError("Capture text is required.");

        Assert.Equal(ItemCollection.Today, viewModel.Collection);
        Assert.Equal("Capture text is required.", viewModel.Error);
    }

    [Theory]
    [InlineData(0, ItemType.Task)]
    [InlineData(1, ItemType.Note)]
    [InlineData(2, ItemType.Event)]
    [InlineData(-1, ItemType.Event)]
    [InlineData(3, ItemType.Task)]
    public void AddItemTypePicker_resolves_selected_index_to_item_type(int selectedIndex, ItemType expectedType)
    {
        var type = AddItemTypePickerScreen.ResolveType(selectedIndex);

        Assert.Equal(expectedType, type);
    }

    [Theory]
    [InlineData(-1, 2)]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 0)]
    public void AddItemTypePicker_normalizes_selected_index(int selectedIndex, int expectedIndex)
    {
        var normalizedIndex = AddItemTypePickerScreen.NormalizeSelectedIndex(selectedIndex);

        Assert.Equal(expectedIndex, normalizedIndex);
    }
}
