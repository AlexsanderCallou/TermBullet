namespace TermBullet.Tui.Screens;

public sealed class CalendarViewModel
{
    private CalendarViewModel(
        DateOnly selectedDate,
        IReadOnlyList<CalendarDayCell> monthCells,
        IReadOnlyList<ItemDisplayRow> selectedDayItems)
    {
        SelectedDate = selectedDate;
        MonthCells = monthCells;
        SelectedDayItems = selectedDayItems;
    }

    public DateOnly SelectedDate { get; }

    public IReadOnlyList<CalendarDayCell> MonthCells { get; }

    public IReadOnlyList<ItemDisplayRow> SelectedDayItems { get; }

    public static CalendarViewModel Build(IReadOnlyCollection<ItemDisplayRow> rows, DateOnly selectedDate)
    {
        var firstDay = new DateOnly(selectedDate.Year, selectedDate.Month, 1);
        var daysInMonth = DateTime.DaysInMonth(selectedDate.Year, selectedDate.Month);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var cells = new List<CalendarDayCell>(daysInMonth);

        for (var day = 1; day <= daysInMonth; day++)
        {
            var date = new DateOnly(selectedDate.Year, selectedDate.Month, day);
            cells.Add(new CalendarDayCell
            {
                Date = date,
                TaskCount = 0,
                EventCount = rows.Count(row => IsEventOnDate(row, date)),
                IsToday = date == today,
                IsSelected = date == selectedDate
            });
        }

        var selectedItems = rows
            .Where(row => IsEventOnDate(row, selectedDate))
            .OrderBy(row => row.Type.Equals("event", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .ThenBy(row => row.ScheduledAt)
            .ThenBy(row => row.PublicRef)
            .ToArray();

        return new CalendarViewModel(selectedDate, cells, selectedItems);
    }

    public static IReadOnlyList<ItemDisplayRow> BuildNoteRows(IReadOnlyCollection<ItemDisplayRow> rows) =>
        rows
            .Where(row => row.Type.Equals("note", StringComparison.OrdinalIgnoreCase))
            .OrderBy(row => row.PublicRef)
            .ToArray();

    public static DateOnly MoveSelectedDate(DateOnly selectedDate, int days) =>
        selectedDate.AddDays(days);

    public static DateOnly MoveSelectedMonth(DateOnly selectedDate, int months) =>
        selectedDate.AddMonths(months);

    private static bool IsEventOnDate(ItemDisplayRow row, DateOnly date) =>
        row.Type.Equals("event", StringComparison.OrdinalIgnoreCase)
        && row.ScheduledAt is not null
        && DateOnly.FromDateTime(row.ScheduledAt.Value.DateTime) == date;
}
