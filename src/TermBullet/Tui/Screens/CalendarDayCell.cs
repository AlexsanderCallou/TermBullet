namespace TermBullet.Tui.Screens;

public sealed class CalendarDayCell
{
    public required DateOnly Date { get; init; }
    public required int TaskCount { get; init; }
    public required int EventCount { get; init; }
    public required bool IsToday { get; init; }
    public required bool IsSelected { get; init; }
}
