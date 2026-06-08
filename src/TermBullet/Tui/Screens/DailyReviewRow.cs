namespace TermBullet.Tui.Screens;

public sealed record DailyReviewRow(
    ItemDisplayRow Item,
    DateOnly LastTodayPlacementDate)
{
    public string PublicRef => Item.PublicRef;
}
