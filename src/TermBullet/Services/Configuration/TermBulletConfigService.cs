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

        return new TermBulletConfig(document.DataRoot, ToAiConfiguration(document.Ai));
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
            var document = new ConfigDocument
            {
                DataRoot = config.DataRoot,
                Ai = ToAiDocument(config.Ai)
            };
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

        [JsonPropertyName("ai")]
        public AiConfigDocument? Ai { get; set; }
    }

    private sealed class AiConfigDocument
    {
        [JsonPropertyName("active_profile")]
        public string? ActiveProfile { get; set; }

        [JsonPropertyName("profiles")]
        public Dictionary<string, AiProfileDocument>? Profiles { get; set; }
    }

    private sealed class AiProfileDocument
    {
        [JsonPropertyName("provider")]
        public string? Provider { get; set; }

        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("base_url")]
        public string? BaseUrl { get; set; }

        [JsonPropertyName("api_key_source")]
        public string? ApiKeySource { get; set; }

        [JsonPropertyName("api_key_env")]
        public string? ApiKeyEnv { get; set; }

        [JsonPropertyName("api_key")]
        public string? ApiKey { get; set; }

        [JsonPropertyName("timeout_seconds")]
        public int? TimeoutSeconds { get; set; }
    }

    private static AiConfiguration? ToAiConfiguration(AiConfigDocument? document)
    {
        if (document is null)
        {
            return null;
        }

        var profiles = new Dictionary<string, AiProfile>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in document.Profiles ?? [])
        {
            if (string.IsNullOrWhiteSpace(pair.Key)
                || pair.Value is null
                || string.IsNullOrWhiteSpace(pair.Value.Provider)
                || string.IsNullOrWhiteSpace(pair.Value.Model))
            {
                continue;
            }

            profiles[pair.Key.Trim()] = new AiProfile(
                pair.Value.Provider.Trim(),
                pair.Value.Model.Trim(),
                NormalizeOptional(pair.Value.BaseUrl),
                NormalizeOptional(pair.Value.ApiKeySource) ?? "environment",
                NormalizeOptional(pair.Value.ApiKeyEnv),
                NormalizeOptional(pair.Value.ApiKey),
                pair.Value.TimeoutSeconds);
        }

        return new AiConfiguration(NormalizeOptional(document.ActiveProfile), profiles);
    }

    private static AiConfigDocument? ToAiDocument(AiConfiguration? configuration)
    {
        if (configuration is null)
        {
            return null;
        }

        return new AiConfigDocument
        {
            ActiveProfile = NormalizeOptional(configuration.ActiveProfile),
            Profiles = configuration.Profiles
                .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    pair => pair.Key,
                    pair => new AiProfileDocument
                    {
                        Provider = pair.Value.Provider,
                        Model = pair.Value.Model,
                        BaseUrl = NormalizeOptional(pair.Value.BaseUrl),
                        ApiKeySource = NormalizeOptional(pair.Value.ApiKeySource) ?? "environment",
                        ApiKeyEnv = NormalizeOptional(pair.Value.ApiKeyEnv),
                        ApiKey = NormalizeOptional(pair.Value.ApiKey),
                        TimeoutSeconds = pair.Value.TimeoutSeconds
                    },
                    StringComparer.OrdinalIgnoreCase)
        };
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
