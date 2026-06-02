using TermBullet.Domain.Items;

namespace TermBullet.Tui.Screens;

public sealed class TuiAddItemViewModel
{
    private TuiAddItemViewModel(ItemType type, ItemCollection collection, string? error)
    {
        Type = type;
        Collection = collection;
        Error = error;
    }

    public ItemType Type { get; }

    public ItemCollection Collection { get; }

    public string? Error { get; }

    public IReadOnlyList<string> Examples =>
        Type switch
        {
            ItemType.Note =>
            [
                "investigate stacktrace",
                "decision keep V1 local-first",
                "Terminal.Gui research notes"
            ],
            ItemType.Event =>
            [
                "team sync on 2026-05-12",
                "dentist appointment on 2026-05-12",
                "release demo on 2026-05-15"
            ],
            _ =>
            [
                "fix jwt authentication",
                "review pull request",
                "write release notes for 2026-05-12"
            ]
        };

    public static TuiAddItemViewModel ForMainDashboard() =>
        ForType(ItemType.Task);

    public static TuiAddItemViewModel ForType(ItemType type) =>
        new(type, ResolveDefaultCollection(type), error: null);

    public TuiAddItemViewModel WithError(string error) =>
        new(Type, Collection, error);

    private static ItemCollection ResolveDefaultCollection(ItemType type) =>
        type switch
        {
            ItemType.Note => ItemCollection.Notes,
            ItemType.Event => ItemCollection.Events,
            _ => ItemCollection.Today
        };
}
