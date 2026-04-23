namespace TermBullet.Application.Ports;

public interface IMonthRolloverService
{
    Task RunAutomaticMonthRolloverAsync(CancellationToken cancellationToken = default);
}
