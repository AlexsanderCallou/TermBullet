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
    public void ForMainDashboard_ProvidesTaskNoteAndEventExamples()
    {
        var viewModel = TuiAddItemViewModel.ForMainDashboard();

        Assert.Contains(viewModel.Examples, line => line.Contains("- Review pull request", StringComparison.Ordinal));
        Assert.Contains(viewModel.Examples, line => line.Contains(". Investigate stacktrace", StringComparison.Ordinal));
        Assert.Contains(viewModel.Examples, line => line.Contains("o Team sync at 16:00", StringComparison.Ordinal));
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
