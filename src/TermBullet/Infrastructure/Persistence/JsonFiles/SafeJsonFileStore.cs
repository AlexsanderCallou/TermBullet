using System.Text.Json;

namespace TermBullet.Infrastructure.Persistence.JsonFiles;

public sealed class SafeJsonFileStore
{
    public async Task WriteAsync(
        string filePath,
        string backupPath,
        string jsonContent,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(backupPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(jsonContent);

        EnsureValidJson(jsonContent);
        EnsureDirectory(filePath);
        EnsureDirectory(backupPath);

        var tempPath = $"{filePath}.{Guid.NewGuid():N}.tmp";
        await File.WriteAllTextAsync(tempPath, jsonContent, cancellationToken);

        try
        {
            if (File.Exists(filePath))
            {
                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }

                File.Copy(filePath, backupPath);
                File.Replace(tempPath, filePath, destinationBackupFileName: null, ignoreMetadataErrors: true);
            }
            else
            {
                File.Move(tempPath, filePath);
            }
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    public async Task<string> ReadOrRecoverAsync(
        string filePath,
        string backupPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(backupPath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Monthly file not found.", filePath);
        }

        var mainContent = await File.ReadAllTextAsync(filePath, cancellationToken);
        if (IsValidJson(mainContent))
        {
            return mainContent;
        }

        if (!File.Exists(backupPath))
        {
            throw new InvalidDataException("Monthly file is corrupted and no backup exists.");
        }

        var backupContent = await File.ReadAllTextAsync(backupPath, cancellationToken);
        if (!IsValidJson(backupContent))
        {
            throw new InvalidDataException("Both monthly file and backup are corrupted.");
        }

        await ReplaceMainFileFromRecoveryAsync(filePath, backupContent, cancellationToken);
        return backupContent;
    }

    private static void EnsureDirectory(string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new ArgumentException("Path must include a directory.", nameof(path));
        }

        Directory.CreateDirectory(directory);
    }

    private static bool IsValidJson(string json)
    {
        try
        {
            using var _ = JsonDocument.Parse(json);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static void EnsureValidJson(string json)
    {
        if (!IsValidJson(json))
        {
            throw new InvalidDataException("Invalid JSON content.");
        }
    }

    private static async Task ReplaceMainFileFromRecoveryAsync(
        string filePath,
        string jsonContent,
        CancellationToken cancellationToken)
    {
        EnsureDirectory(filePath);
        var tempPath = $"{filePath}.{Guid.NewGuid():N}.tmp";
        await File.WriteAllTextAsync(tempPath, jsonContent, cancellationToken);

        try
        {
            if (File.Exists(filePath))
            {
                File.Replace(tempPath, filePath, destinationBackupFileName: null, ignoreMetadataErrors: true);
            }
            else
            {
                File.Move(tempPath, filePath);
            }
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }
}
