using TermBullet.Application.Ports;

namespace TermBullet.Application.History;

public sealed class ClearStoredHistoryUseCase(
    IHistoryMaintenanceService historyMaintenanceService,
    IClock clock)
{
    public Task ExecuteAsync(
        ClearStoredHistoryRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.All)
        {
            if (request.Month is not null || request.Year is not null)
            {
                throw new ArgumentException("All-history cleanup cannot be combined with a specific month.");
            }

            return historyMaintenanceService.ClearAllAsync(cancellationToken);
        }

        if (request.Month is null && request.Year is null)
        {
            var now = clock.UtcNow;
            return historyMaintenanceService.ClearMonthAsync(now.Month, now.Year, cancellationToken);
        }

        if (request.Month is null || request.Year is null)
        {
            throw new ArgumentException("Month and year must be provided together.");
        }

        return historyMaintenanceService.ClearMonthAsync(request.Month.Value, request.Year.Value, cancellationToken);
    }
}
