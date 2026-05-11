namespace TermBullet.Services.Maintenance;

public interface IMonthRolloverService
{
    Task RunAutomaticMonthRolloverAsync(CancellationToken cancellationToken = default);
}
