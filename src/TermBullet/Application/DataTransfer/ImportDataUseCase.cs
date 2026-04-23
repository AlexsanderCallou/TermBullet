using TermBullet.Application.Ports;

namespace TermBullet.Application.DataTransfer;

public sealed class ImportDataUseCase(IDataTransferService dataTransferService)
{
    public Task ExecuteAsync(
        ImportDataRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!string.Equals(request.Format, "json", StringComparison.OrdinalIgnoreCase))
        {
            throw new NotSupportedException($"Unsupported import format: {request.Format}.");
        }

        return dataTransferService.ImportAsync(request.InputPath, cancellationToken);
    }
}
