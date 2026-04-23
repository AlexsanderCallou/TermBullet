using System.Text.Json;
using System.Text.Json.Serialization;

using TermBullet.Application.Ports;

namespace TermBullet.Infrastructure.Persistence.JsonFiles;

public sealed class LocalSettingsStore(
    string projectRootPath,
    SafeJsonFileStore fileStore) : ISettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public string SettingsPath => GetSettingsPath();

    public async Task<string?> GetAsync(
        string key,
        string profile = "default",
        CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        var document = await ReadAsync(cancellationToken);
        var normalizedProfile = NormalizeProfile(profile);

        return document.Profiles.TryGetValue(normalizedProfile, out var settings)
            && settings.TryGetValue(key, out var value)
                ? value
                : null;
    }

    public async Task<IReadOnlyDictionary<string, string>> ListAsync(
        string profile = "default",
        CancellationToken cancellationToken = default)
    {
        var document = await ReadAsync(cancellationToken);
        var normalizedProfile = NormalizeProfile(profile);

        return document.Profiles.TryGetValue(normalizedProfile, out var settings)
            ? new Dictionary<string, string>(settings, StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    public async Task SetAsync(
        string key,
        string value,
        string profile = "default",
        CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var document = await ReadAsync(cancellationToken);
        var normalizedProfile = NormalizeProfile(profile);
        if (!document.Profiles.TryGetValue(normalizedProfile, out var settings))
        {
            settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            document.Profiles[normalizedProfile] = settings;
        }

        settings[key] = value;
        await WriteAsync(document, cancellationToken);
    }

    private async Task<SettingsDocument> ReadAsync(CancellationToken cancellationToken)
    {
        var settingsPath = GetSettingsPath();
        if (!File.Exists(settingsPath))
        {
            return new SettingsDocument();
        }

        var backupPath = GetBackupPath();
        var json = await fileStore.ReadOrRecoverAsync(settingsPath, backupPath, cancellationToken);
        return JsonSerializer.Deserialize<SettingsDocument>(json, JsonOptions) ?? new SettingsDocument();
    }

    private async Task WriteAsync(SettingsDocument document, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(document, JsonOptions);
        await fileStore.WriteAsync(GetSettingsPath(), GetBackupPath(), json, cancellationToken);
    }

    private string GetSettingsPath() => Path.Combine(projectRootPath, "data", "settings.json");

    private string GetBackupPath() => Path.Combine(projectRootPath, "data", "settings.backup.json");

    private static void ValidateKey(string key) => ArgumentException.ThrowIfNullOrWhiteSpace(key);

    private static string NormalizeProfile(string profile)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(profile);
        return profile.Trim();
    }

    private sealed class SettingsDocument
    {
        [JsonPropertyName("profiles")]
        public Dictionary<string, Dictionary<string, string>> Profiles { get; set; } =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["default"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            };
    }
}
