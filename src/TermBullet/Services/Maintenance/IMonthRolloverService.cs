using TermBullet.Services.Maintenance;
namespace TermBullet.Services.Maintenance;

public interface IMonthRolloverService
{
    Task RunAutomaticMonthRolloverAsync(CancellationToken cancellationToken = default);
}
