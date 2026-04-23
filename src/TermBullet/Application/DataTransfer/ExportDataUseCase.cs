using TermBullet.Application.Ports;

namespace TermBullet.Application.DataTransfer;

public sealed class ExportDataUseCase(IDataTransferService dataTransferService)
{
    public Task ExecuteAsync(
        ExportDataRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!string.Equals(request.Format, "json", StringComparison.OrdinalIgnoreCase))
        {
            throw new NotSupportedException($"Unsupported export format: {request.Format}.");
        }

        return dataTransferService.ExportAsync(request.OutputPath, cancellationToken);
    }
}
