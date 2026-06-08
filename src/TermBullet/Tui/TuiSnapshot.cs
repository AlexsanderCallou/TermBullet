using TermBullet.Application.Items;
using TermBullet.Application.Tags;

namespace TermBullet.Tui;

public sealed record TuiSnapshot(
    IReadOnlyCollection<ItemResult> TodayItems,
    IReadOnlyCollection<ItemResult> WeekItems,
    IReadOnlyCollection<ItemResult> MonthItems,
    IReadOnlyCollection<ItemResult> BacklogItems,
    IReadOnlyCollection<DailyReviewItemResult> DailyReviewItems,
    IReadOnlyCollection<ItemResult> CurrentItems,
    IReadOnlyCollection<ItemResult> AllItems,
    IReadOnlyCollection<TagCatalogResult> Tags);
