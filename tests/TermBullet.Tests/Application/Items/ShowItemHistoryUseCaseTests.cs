using TermBullet.Application.Items;
using TermBullet.Repositories.Interfaces;

namespace TermBullet.Tests.Application.Items;

public sealed class ShowItemHistoryUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_returns_history_entries_with_summaries()
    {
        var occurredAt = new DateTimeOffset(2026, 5, 9, 10, 45, 0, TimeSpan.Zero);
        var repository = new FakeHistoryReader(
            new ItemHistoryEntry(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "t-0526-1",
                "migrate",
                occurredAt,
                """{"from_collection":"today","to_collection":"week"}"""));
        var useCase = new ShowItemHistoryUseCase(repository);

        var result = await useCase.ExecuteAsync("t-0526-1");

        var entry = Assert.Single(result);
        Assert.Equal(occurredAt, entry.OccurredAt);
        Assert.Equal("migrate", entry.EventType);
        Assert.Equal("migrate: today -> week", entry.Summary);
    }

    private sealed class FakeHistoryReader(params ItemHistoryEntry[] entries) : IItemHistoryReader
    {
        public Task<IReadOnlyCollection<ItemHistoryEntry>> ListHistoryByPublicRefAsync(
            string publicRef,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<ItemHistoryEntry>>(
                entries
                    .Where(entry => entry.PublicRef == publicRef)
                    .ToArray());
        }
    }
}
