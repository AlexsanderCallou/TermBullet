using TermBullet.Application.Ports;

namespace TermBullet.Application.Startup;

public sealed class RunStartupMaintenanceUseCase(
    IClock clock,
    IMonthRolloverService monthRolloverService)
{
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return clock.UtcNow.Day == 1
            ? monthRolloverService.RunAutomaticMonthRolloverAsync(cancellationToken)
            : Task.CompletedTask;
    }
}
