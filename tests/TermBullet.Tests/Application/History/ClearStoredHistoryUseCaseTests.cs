using TermBullet.Application.History;
using TermBullet.Application.Ports;

namespace TermBullet.Tests.Application.History;

public sealed class ClearStoredHistoryUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_clears_current_month_when_request_has_no_scope()
    {
        var clock = new FixedClock(new DateTimeOffset(2026, 4, 23, 12, 0, 0, TimeSpan.Zero));
        var service = new FakeHistoryMaintenanceService();
        var useCase = new ClearStoredHistoryUseCase(service, clock);

        await useCase.ExecuteAsync(new ClearStoredHistoryRequest());

        Assert.Equal((4, 2026), service.ClearedMonth);
    }

    [Fact]
    public async Task ExecuteAsync_clears_specific_month_when_request_includes_month_and_year()
    {
        var clock = new FixedClock(new DateTimeOffset(2026, 4, 23, 12, 0, 0, TimeSpan.Zero));
        var service = new FakeHistoryMaintenanceService();
        var useCase = new ClearStoredHistoryUseCase(service, clock);

        await useCase.ExecuteAsync(new ClearStoredHistoryRequest(Month: 5, Year: 2026));

        Assert.Equal((5, 2026), service.ClearedMonth);
    }

    [Fact]
    public async Task ExecuteAsync_clears_all_months_when_request_requires_it()
    {
        var clock = new FixedClock(new DateTimeOffset(2026, 4, 23, 12, 0, 0, TimeSpan.Zero));
        var service = new FakeHistoryMaintenanceService();
        var useCase = new ClearStoredHistoryUseCase(service, clock);

        await useCase.ExecuteAsync(new ClearStoredHistoryRequest(All: true));

        Assert.True(service.ClearAllCalled);
    }

    [Fact]
    public async Task ExecuteAsync_rejects_partial_month_scope()
    {
        var clock = new FixedClock(new DateTimeOffset(2026, 4, 23, 12, 0, 0, TimeSpan.Zero));
        var service = new FakeHistoryMaintenanceService();
        var useCase = new ClearStoredHistoryUseCase(service, clock);

        await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.ExecuteAsync(new ClearStoredHistoryRequest(Month: 4)));
    }

    private sealed class FakeHistoryMaintenanceService : IHistoryMaintenanceService
    {
        public (int Month, int Year)? ClearedMonth { get; private set; }

        public bool ClearAllCalled { get; private set; }

        public Task ClearMonthAsync(int month, int year, CancellationToken cancellationToken = default)
        {
            ClearedMonth = (month, year);
            return Task.CompletedTask;
        }

        public Task ClearAllAsync(CancellationToken cancellationToken = default)
        {
            ClearAllCalled = true;
            return Task.CompletedTask;
        }
    }

    private sealed class FixedClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }
}
