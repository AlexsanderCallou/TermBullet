using TermBullet.Core.Items;
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
}
