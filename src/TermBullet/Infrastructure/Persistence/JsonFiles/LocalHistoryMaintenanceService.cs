using System.Text.Json;
using System.Text.Json.Nodes;
using TermBullet.Application.Ports;

namespace TermBullet.Infrastructure.Persistence.JsonFiles;

public sealed class LocalHistoryMaintenanceService(
    string projectRootPath,
    MonthlyJsonFilePathResolver pathResolver,
    SafeJsonFileStore fileStore) : IHistoryMaintenanceService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public Task ClearMonthAsync(int month, int year, CancellationToken cancellationToken = default)
    {
        var monthlyPath = pathResolver.ResolveMonthlyFilePath(year, month);
        return ClearFileHistoryIfExistsAsync(monthlyPath, cancellationToken);
    }

    public async Task ClearAllAsync(CancellationToken cancellationToken = default)
    {
        var dataRootPath = Path.Combine(projectRootPath, "data");
        if (!Directory.Exists(dataRootPath))
        {
            return;
        }

        var monthlyPaths = Directory
            .EnumerateFiles(dataRootPath, "data_??_????.json", SearchOption.AllDirectories)
            .Where(path => !path.EndsWith(".backup.json", StringComparison.OrdinalIgnoreCase))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (var monthlyPath in monthlyPaths)
        {
            await ClearFileHistoryIfExistsAsync(monthlyPath, cancellationToken);
        }
    }

    private async Task ClearFileHistoryIfExistsAsync(string monthlyPath, CancellationToken cancellationToken)
    {
        if (!File.Exists(monthlyPath))
        {
            return;
        }

        var backupPath = GetBackupPath(monthlyPath);
        var json = await fileStore.ReadOrRecoverAsync(monthlyPath, backupPath, cancellationToken);
        JsonNode? rootNode;
        try
        {
            rootNode = JsonNode.Parse(json);
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException($"Invalid monthly JSON file: {monthlyPath}.", exception);
        }

        if (rootNode is not JsonObject rootObject)
        {
            throw new InvalidDataException($"Monthly JSON root must be an object: {monthlyPath}.");
        }

        rootObject["history"] = new JsonArray();
        var updatedJson = rootObject.ToJsonString(JsonOptions);
        await fileStore.WriteAsync(monthlyPath, backupPath, updatedJson, cancellationToken);
    }

    private static string GetBackupPath(string monthlyPath)
    {
        var directory = Path.GetDirectoryName(monthlyPath)
            ?? throw new InvalidOperationException("Monthly path must include a directory.");
        var fileName = Path.GetFileNameWithoutExtension(monthlyPath);
        return Path.Combine(directory, $"{fileName}.backup.json");
    }
}
