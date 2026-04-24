using TermBullet.Application.Items;

namespace TermBullet.Tui;

public sealed record TuiSnapshot(
    IReadOnlyCollection<ItemResult> TodayItems,
    IReadOnlyCollection<ItemResult> WeekItems,
    IReadOnlyCollection<ItemResult> BacklogItems,
    IReadOnlyCollection<ItemResult> AllItems,
    IReadOnlyDictionary<string, string> Configuration);
