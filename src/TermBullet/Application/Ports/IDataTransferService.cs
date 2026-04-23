namespace TermBullet.Application.Ports;

public interface IDataTransferService
{
    Task ExportAsync(string outputPath, CancellationToken cancellationToken = default);

    Task ImportAsync(string inputPath, CancellationToken cancellationToken = default);
}
