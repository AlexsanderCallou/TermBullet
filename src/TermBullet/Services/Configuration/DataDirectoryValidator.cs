namespace TermBullet.Services.Configuration;

public sealed class DataDirectoryValidator
{
    public string ValidateAndPrepare(string dataRoot)
    {
        if (string.IsNullOrWhiteSpace(dataRoot))
        {
            throw new ArgumentException("Data directory is required.", nameof(dataRoot));
        }

        var fullPath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(dataRoot.Trim()));
        var dataPath = Path.Combine(fullPath, "data");
        var probePath = Path.Combine(dataPath, $".termbullet-write-test-{Guid.NewGuid():N}.tmp");

        try
        {
            Directory.CreateDirectory(dataPath);
            File.WriteAllText(probePath, "ok");
            _ = File.ReadAllText(probePath);
            File.Delete(probePath);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            throw new InvalidOperationException(
                $"Data directory is not writable: {fullPath}. Choose another directory or adjust permissions.",
                exception);
        }
        finally
        {
            if (File.Exists(probePath))
            {
                File.Delete(probePath);
            }
        }

        return fullPath;
    }
}
