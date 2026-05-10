using TermBullet.Tui.Screens;

namespace TermBullet.Tests.Tui;

public sealed class CalendarViewModelTests
{
    [Fact]
    public void Build_counts_tasks_and_events_for_selected_month()
    {
        var selectedDate = new DateOnly(2026, 5, 9);
        var rows = new[]
        {
            MakeRow("t-0526-1", "task", plannedFor: selectedDate),
            MakeRow("e-0526-1", "event", scheduledAt: new DateTimeOffset(2026, 5, 9, 16, 0, 0, TimeSpan.Zero)),
            MakeRow("n-0526-1", "note")
        };

        var vm = CalendarViewModel.Build(rows, selectedDate);

        var cell = Assert.Single(vm.MonthCells, day => day.Date == selectedDate);
        Assert.Equal(1, cell.TaskCount);
        Assert.Equal(1, cell.EventCount);
        Assert.Equal(2, vm.SelectedDayItems.Count);
        Assert.DoesNotContain(vm.SelectedDayItems, item => item.Type == "note");
    }

    [Fact]
    public void BuildNoteRows_returns_only_notes()
    {
        var rows = new[]
        {
            MakeRow("t-0526-1", "task"),
            MakeRow("n-0526-1", "note"),
            MakeRow("e-0526-1", "event")
        };

        var notes = CalendarViewModel.BuildNoteRows(rows);

        var note = Assert.Single(notes);
        Assert.Equal("n-0526-1", note.PublicRef);
    }

    private static ItemDisplayRow MakeRow(
        string publicRef,
        string type,
        DateOnly? plannedFor = null,
        DateTimeOffset? scheduledAt = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            PublicRef = publicRef,
            Symbol = type == "note" ? "(.)" : type == "event" ? "(o)" : "[ ]",
            Type = type,
            Status = "open",
            Content = "Item",
            Description = null,
            Priority = "none",
            Collection = "today",
            Tags = [],
            PlannedFor = plannedFor,
            ScheduledAt = scheduledAt,
            Version = 1,
            CreatedAt = new DateTimeOffset(2026, 5, 1, 8, 0, 0, TimeSpan.Zero),
            UpdatedAt = new DateTimeOffset(2026, 5, 1, 8, 0, 0, TimeSpan.Zero)
        };
}
