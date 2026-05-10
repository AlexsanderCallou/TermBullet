using TermBullet.Services.History;
namespace TermBullet.Services.History;

public interface IHistoryMaintenanceService
{
    Task ClearMonthAsync(int month, int year, CancellationToken cancellationToken = default);

    Task ClearAllAsync(CancellationToken cancellationToken = default);
}
