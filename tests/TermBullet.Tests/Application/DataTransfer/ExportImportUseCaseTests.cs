using TermBullet.Application.DataTransfer;
using TermBullet.Application.Ports;

namespace TermBullet.Tests.Application.DataTransfer;

public sealed class ExportImportUseCaseTests
{
    [Fact]
    public async Task ExportDataUseCase_calls_service_for_json_format()
    {
        var service = new FakeDataTransferService();
        var useCase = new ExportDataUseCase(service);

        await useCase.ExecuteAsync(new ExportDataRequest("C:\\temp\\backup.json"));

        Assert.Equal("C:\\temp\\backup.json", service.ExportedPath);
    }

    [Fact]
    public async Task ExportDataUseCase_rejects_unsupported_format()
    {
        var service = new FakeDataTransferService();
        var useCase = new ExportDataUseCase(service);

        await Assert.ThrowsAsync<NotSupportedException>(
            () => useCase.ExecuteAsync(new ExportDataRequest("C:\\temp\\backup.zip", "zip")));
    }

    [Fact]
    public async Task ImportDataUseCase_calls_service_for_json_format()
    {
        var service = new FakeDataTransferService();
        var useCase = new ImportDataUseCase(service);

        await useCase.ExecuteAsync(new ImportDataRequest("C:\\temp\\backup.json"));

        Assert.Equal("C:\\temp\\backup.json", service.ImportedPath);
    }

    [Fact]
    public async Task ImportDataUseCase_rejects_unsupported_format()
    {
        var service = new FakeDataTransferService();
        var useCase = new ImportDataUseCase(service);

        await Assert.ThrowsAsync<NotSupportedException>(
            () => useCase.ExecuteAsync(new ImportDataRequest("C:\\temp\\backup.zip", "zip")));
    }

    private sealed class FakeDataTransferService : IDataTransferService
    {
        public string? ExportedPath { get; private set; }

        public string? ImportedPath { get; private set; }

        public Task ExportAsync(string outputPath, CancellationToken cancellationToken = default)
        {
            ExportedPath = outputPath;
            return Task.CompletedTask;
        }

        public Task ImportAsync(string inputPath, CancellationToken cancellationToken = default)
        {
            ImportedPath = inputPath;
            return Task.CompletedTask;
        }
    }
}
