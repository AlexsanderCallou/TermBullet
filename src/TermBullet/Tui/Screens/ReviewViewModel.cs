using TermBullet.Application.Items;
using TermBullet.Core.Items;

namespace TermBullet.Tui.Screens;

public sealed class ReviewViewModel
{
    public ReviewViewModel(IReadOnlyCollection<ItemResult> items)
    {
        Collections =
        [
            $"> Daily ({items.Count(item => item.Collection == ItemCollection.Today)})",
            $"  Weekly ({items.Count(item => item.Collection == ItemCollection.Week)})",
            $"  Monthly ({items.Count(item => item.Collection == ItemCollection.Monthly)})"
        ];
        Summary =
        [
            $"open: {items.Count(item => item.Status == ItemStatus.Open)}",
            $"done: {items.Count(item => item.Status == ItemStatus.Done)}",
            $"migrated: {items.Count(item => item.Status == ItemStatus.Migrated)}",
            $"events: {items.Count(item => item.Type == ItemType.Event)}"
        ];
        WhatMoved = items
            .Where(item => item.Status is ItemStatus.Done or ItemStatus.Migrated)
            .Take(3)
            .Select(item => item.Content)
            .DefaultIfEmpty("no completed movement yet")
            .ToArray();
        WhatBlocked = items
            .Where(item => item.Status == ItemStatus.Cancelled)
            .Take(3)
            .Select(item => item.Content)
            .DefaultIfEmpty("no blockers recorded")
            .ToArray();
        NextCycle =
        [
            items.Any(item => item.Priority == Priority.High)
                ? "prioritize high priority carry-over"
                : "capture one concrete weekly focus",
            "move unclear scope back to backlog",
            "protect deep work from noisy context switches"
        ];
    }

    public IReadOnlyList<string> Collections { get; }

    public IReadOnlyList<string> Summary { get; }

    public IReadOnlyList<string> WhatMoved { get; }

    public IReadOnlyList<string> WhatBlocked { get; }

    public IReadOnlyList<string> NextCycle { get; }
}

