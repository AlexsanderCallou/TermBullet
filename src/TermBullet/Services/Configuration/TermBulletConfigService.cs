using System.Text.Json;
using System.Text.Json.Serialization;

namespace TermBullet.Services.Configuration;

public sealed class TermBulletConfigService(string installDirectory)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public string InstallDirectory { get; } = Path.GetFullPath(installDirectory);

    public string ConfigPath => Path.Combine(InstallDirectory, "conf.json");

    public async Task<TermBulletConfig?> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(ConfigPath))
        {
            return null;
        }

        ConfigDocument? document;
        try
        {
            var json = await File.ReadAllTextAsync(ConfigPath, cancellationToken);
            document = JsonSerializer.Deserialize<ConfigDocument>(json, JsonOptions);
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException($"conf.json is malformed: {ConfigPath}", exception);
        }

        if (document is null || string.IsNullOrWhiteSpace(document.DataRoot))
        {
            throw new InvalidOperationException($"conf.json must include a non-empty data_root value: {ConfigPath}");
        }

        return new TermBulletConfig(document.DataRoot);
    }

    public async Task SaveAsync(TermBulletConfig config, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(config);
        if (string.IsNullOrWhiteSpace(config.DataRoot))
        {
            throw new ArgumentException("data_root is required.", nameof(config));
        }

        try
        {
            Directory.CreateDirectory(InstallDirectory);
            var document = new ConfigDocument { DataRoot = config.DataRoot };
            var json = JsonSerializer.Serialize(document, JsonOptions);
            await File.WriteAllTextAsync(ConfigPath, json, cancellationToken);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            throw new InvalidOperationException(
                $"Cannot write TermBullet config at {ConfigPath}. Adjust permissions for the installation directory.",
                exception);
        }
    }

    private sealed class ConfigDocument
    {
        [JsonPropertyName("data_root")]
        public string? DataRoot { get; set; }
    }
}
