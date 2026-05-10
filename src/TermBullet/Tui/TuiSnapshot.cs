using TermBullet.Application.Items;
using TermBullet.Application.Tags;

namespace TermBullet.Tui;

public sealed record TuiSnapshot(
    IReadOnlyCollection<ItemResult> TodayItems,
    IReadOnlyCollection<ItemResult> WeekItems,
    IReadOnlyCollection<ItemResult> BacklogItems,
    IReadOnlyCollection<ItemResult> AllItems,
    IReadOnlyCollection<TagCatalogResult> Tags,
    IReadOnlyDictionary<string, string> Configuration);
