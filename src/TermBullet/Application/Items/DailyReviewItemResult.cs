namespace TermBullet.Application.Items;

public sealed record DailyReviewItemResult(
    ItemResult Item,
    DateOnly LastTodayPlacementDate);
