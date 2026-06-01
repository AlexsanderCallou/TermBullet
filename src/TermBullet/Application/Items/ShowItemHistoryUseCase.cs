using System.Text.Json;
using TermBullet.Repositories.Interfaces;

namespace TermBullet.Application.Items;

public sealed class ShowItemHistoryUseCase(IItemHistoryReader historyReader)
{
    public async Task<IReadOnlyCollection<ItemHistoryEntryResult>> ExecuteAsync(
        string publicRef,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(publicRef);

        var entries = await historyReader.ListHistoryByPublicRefAsync(publicRef, cancellationToken);
        return entries
            .OrderBy(entry => entry.OccurredAt)
            .Select(entry => new ItemHistoryEntryResult(
                entry.OccurredAt,
                entry.EventType,
                BuildSummary(entry)))
            .ToArray();
    }

    private static string BuildSummary(ItemHistoryEntry entry)
    {
        if (string.IsNullOrWhiteSpace(entry.DataJson))
        {
            return entry.EventType;
        }

        try
        {
            using var document = JsonDocument.Parse(entry.DataJson);
            var root = document.RootElement;
            if (root.TryGetProperty("from_collection", out var from)
                && root.TryGetProperty("to_collection", out var to))
            {
                return $"{entry.EventType}: {from.GetString()} -> {to.GetString()}";
            }

            if (root.TryGetProperty("status", out var status))
            {
                return $"{entry.EventType}: {status.GetString()}";
            }

            if (root.TryGetProperty("content", out var content))
            {
                return $"{entry.EventType}: {content.GetString()}";
            }

            if (root.TryGetProperty("snapshot", out var snapshot)
                && snapshot.TryGetProperty("content", out var snapshotContent))
            {
                return $"{entry.EventType}: {snapshotContent.GetString()}";
            }
        }
        catch (JsonException)
        {
            return entry.EventType;
        }

        return entry.EventType;
    }
}
