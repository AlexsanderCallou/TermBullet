using TermBullet.Application.Ports;
using TermBullet.Application.Startup;

namespace TermBullet.Tests.Application.Startup;

public sealed class RunStartupMaintenanceUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_runs_month_rollover_on_first_day_of_month()
    {
        var service = new FakeMonthRolloverService();
        var useCase = new RunStartupMaintenanceUseCase(
            new FixedClock(new DateTimeOffset(2026, 5, 1, 8, 0, 0, TimeSpan.Zero)),
            service);

        await useCase.ExecuteAsync();

        Assert.True(service.WasCalled);
    }

    [Fact]
    public async Task ExecuteAsync_skips_month_rollover_on_other_days()
    {
        var service = new FakeMonthRolloverService();
        var useCase = new RunStartupMaintenanceUseCase(
            new FixedClock(new DateTimeOffset(2026, 5, 2, 8, 0, 0, TimeSpan.Zero)),
            service);

        await useCase.ExecuteAsync();

        Assert.False(service.WasCalled);
    }

    private sealed class FakeMonthRolloverService : IMonthRolloverService
    {
        public bool WasCalled { get; private set; }

        public Task RunAutomaticMonthRolloverAsync(CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.CompletedTask;
        }
    }

    private sealed class FixedClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }
}
