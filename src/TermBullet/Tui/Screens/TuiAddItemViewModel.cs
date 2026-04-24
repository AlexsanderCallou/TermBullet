using TermBullet.Core.Items;
using TermBullet.Tui.Navigation;

namespace TermBullet.Tui.Screens;

public sealed class TuiAddItemViewModel
{
    private TuiAddItemViewModel(TuiScreen sourceScreen, ItemCollection collection, string? error)
    {
        SourceScreen = sourceScreen;
        Collection = collection;
        Error = error;
    }

    public TuiScreen SourceScreen { get; }

    public ItemCollection Collection { get; }

    public string? Error { get; }

    public IReadOnlyList<string> Examples =>
    [
        "- Review pull request",
        ". Investigate stacktrace",
        "o Team sync at 16:00",
        "- Write release notes #release",
        ". Decision: keep V1 local-first",
        "o Dentist 2026-04-25 09:00"
    ];

    public static TuiAddItemViewModel ForSourceScreen(TuiScreen sourceScreen) =>
        new(sourceScreen, ResolveCollection(sourceScreen), error: null);

    public TuiAddItemViewModel WithError(string error) =>
        new(SourceScreen, Collection, error);

    private static ItemCollection ResolveCollection(TuiScreen sourceScreen) =>
        sourceScreen switch
        {
            TuiScreen.WeeklyPlanning => ItemCollection.Week,
            TuiScreen.BacklogTriage => ItemCollection.Backlog,
            TuiScreen.Review => ItemCollection.Monthly,
            _ => ItemCollection.Today
        };
}
