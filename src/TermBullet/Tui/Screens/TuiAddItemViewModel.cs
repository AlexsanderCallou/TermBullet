using TermBullet.Core.Items;

namespace TermBullet.Tui.Screens;

public sealed class TuiAddItemViewModel
{
    private TuiAddItemViewModel(ItemCollection collection, string? error)
    {
        Collection = collection;
        Error = error;
    }

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

    public static TuiAddItemViewModel ForMainDashboard() =>
        new(ItemCollection.Today, error: null);

    public TuiAddItemViewModel WithError(string error) =>
        new(Collection, error);
}
