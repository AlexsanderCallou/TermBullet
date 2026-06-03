using System.Text;

namespace TermBullet.Services.Configuration;

public sealed class AiConfigurationFileService(string dataRoot)
{
    public const string FileName = ".aiconf";
    private const int DefaultTimeoutSeconds = 180;

    public string FilePath => Path.Combine(dataRoot, FileName);

    public async Task<bool> ExistsAsync(CancellationToken cancellationToken = default) =>
        await Task.FromResult(File.Exists(FilePath));

    public async Task<TermBulletConfig> LoadConfigAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(FilePath))
        {
            throw new InvalidOperationException(
                $"AI configuration file was not found: {FilePath}. Run 'termbullet test-ai' to create a template.");
        }

        var text = await File.ReadAllTextAsync(FilePath, cancellationToken);
        var ai = Parse(text);
        return new TermBulletConfig(dataRoot, ai);
    }

    public async Task<TermBulletConfig> LoadConfigOrCreateTemplateAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(FilePath))
        {
            Directory.CreateDirectory(dataRoot);
            await File.WriteAllTextAsync(FilePath, CreateTemplate(), cancellationToken);
            throw new InvalidOperationException(
                $"AI configuration file was created at {FilePath}. Edit it and run 'termbullet test-ai' again.");
        }

        return await LoadConfigAsync(cancellationToken);
    }

    public async Task SetActiveProfileAsync(string profileName, CancellationToken cancellationToken = default)
    {
        var config = await LoadConfigAsync(cancellationToken);
        if (!config.Ai!.Profiles.ContainsKey(profileName))
        {
            throw new InvalidOperationException($"AI profile not found: {profileName}");
        }

        Directory.CreateDirectory(dataRoot);
        await File.WriteAllTextAsync(
            FilePath,
            Render(config.Ai with { ActiveProfile = profileName }),
            cancellationToken);
    }

    public static AiConfiguration Parse(string text)
    {
        var profiles = new Dictionary<string, AiProfileBuilder>(StringComparer.OrdinalIgnoreCase);
        string? currentName = null;
        var defaultProfiles = new List<string>();

        var lines = text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        for (var index = 0; index < lines.Length; index++)
        {
            var lineNumber = index + 1;
            var line = StripComment(lines[index]).Trim();
            if (line.Length == 0)
            {
                continue;
            }

            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                currentName = line[1..^1].Trim();
                if (currentName.Length == 0)
                {
                    throw new InvalidOperationException($"Invalid AI profile name at line {lineNumber}.");
                }

                profiles[currentName] = new AiProfileBuilder();
                continue;
            }

            if (currentName is null)
            {
                throw new InvalidOperationException($"AI setting outside a profile at line {lineNumber}.");
            }

            var equalsIndex = line.IndexOf('=');
            if (equalsIndex <= 0)
            {
                throw new InvalidOperationException($"Invalid AI setting at line {lineNumber}. Use key=value.");
            }

            var key = line[..equalsIndex].Trim().ToLowerInvariant();
            var value = line[(equalsIndex + 1)..].Trim();
            var builder = profiles[currentName];
            switch (key)
            {
                case "provider":
                    builder.Provider = value;
                    break;
                case "model":
                    builder.Model = value;
                    break;
                case "base_url":
                    builder.BaseUrl = value;
                    break;
                case "api_key":
                    builder.ApiKey = value;
                    builder.ApiKeySource = "literal";
                    break;
                case "api_key_env":
                    builder.ApiKeyEnv = value;
                    builder.ApiKeySource = "environment";
                    break;
                case "default":
                    if (ParseBoolean(value, lineNumber))
                    {
                        defaultProfiles.Add(currentName);
                    }
                    break;
                case "timeout_seconds":
                    builder.TimeoutSeconds = ParsePositiveInteger(value, key, lineNumber);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported AI setting '{key}' at line {lineNumber}.");
            }
        }

        if (profiles.Count == 0)
        {
            throw new InvalidOperationException("AI configuration must include at least one profile.");
        }

        if (defaultProfiles.Count > 1)
        {
            throw new InvalidOperationException("AI configuration must not include more than one default=true profile.");
        }

        var activeProfile = defaultProfiles.Count == 1
            ? defaultProfiles[0]
            : profiles.Count == 1
                ? profiles.Keys.Single()
                : null;

        if (activeProfile is null)
        {
            throw new InvalidOperationException("Multiple AI profiles are configured. Run 'termbullet set-ai <name>'.");
        }

        var builtProfiles = profiles.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.Build(pair.Key),
            StringComparer.OrdinalIgnoreCase);

        return new AiConfiguration(activeProfile, builtProfiles);
    }

    public static string CreateTemplate() =>
        """
        # TermBullet AI configuration
        #
        # This file lives in the TermBullet data folder.
        # Lines starting with # are comments.
        #
        # Each AI model profile starts with [profile-name].
        # If you have more than one profile, set exactly one default=true.
        #
        # Supported providers:
        # - openai-compatible
        #
        # Ollama example:
        # base_url=http://localhost:11434/v1
        # api_key=ollama
        #
        # OpenAI-compatible hosted example:
        # base_url=https://api.openai.com/v1
        # api_key_env=OPENAI_API_KEY

        [local-gemma]
        provider=openai-compatible
        model=gemma3:4b
        base_url=http://localhost:11434/v1
        api_key=ollama
        default=true
        timeout_seconds=180

        [local-llama-fast]
        provider=openai-compatible
        model=llama3.2:1b
        base_url=http://localhost:11434/v1
        api_key=ollama
        timeout_seconds=180

        # [hosted-fast]
        # provider=openai-compatible
        # model=gpt-4.1-mini
        # base_url=https://api.openai.com/v1
        # api_key_env=OPENAI_API_KEY
        # timeout_seconds=180
        """;

    private static string Render(AiConfiguration configuration)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# TermBullet AI configuration");
        builder.AppendLine("# Lines starting with # are comments.");
        builder.AppendLine("# Use 'termbullet set-ai <name>' to change the default profile.");
        builder.AppendLine();

        foreach (var pair in configuration.Profiles.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
        {
            var profile = pair.Value;
            builder.AppendLine($"[{pair.Key}]");
            builder.AppendLine($"provider={profile.Provider}");
            builder.AppendLine($"model={profile.Model}");
            if (!string.IsNullOrWhiteSpace(profile.BaseUrl))
            {
                builder.AppendLine($"base_url={profile.BaseUrl}");
            }

            if (!string.IsNullOrWhiteSpace(profile.ApiKey))
            {
                builder.AppendLine($"api_key={profile.ApiKey}");
            }
            else if (!string.IsNullOrWhiteSpace(profile.ApiKeyEnv))
            {
                builder.AppendLine($"api_key_env={profile.ApiKeyEnv}");
            }

            builder.AppendLine($"default={string.Equals(pair.Key, configuration.ActiveProfile, StringComparison.OrdinalIgnoreCase).ToString().ToLowerInvariant()}");
            builder.AppendLine($"timeout_seconds={profile.TimeoutSeconds ?? DefaultTimeoutSeconds}");
            builder.AppendLine();
        }

        return builder.ToString();
    }

    private static string StripComment(string line)
    {
        var index = line.IndexOf('#');
        return index < 0 ? line : line[..index];
    }

    private static bool ParseBoolean(string value, int lineNumber) =>
        value.Trim().ToLowerInvariant() switch
        {
            "true" or "yes" or "1" => true,
            "false" or "no" or "0" => false,
            _ => throw new InvalidOperationException($"Invalid boolean value at line {lineNumber}: {value}")
        };

    private static int ParsePositiveInteger(string value, string key, int lineNumber)
    {
        if (!int.TryParse(value, out var parsed) || parsed <= 0)
        {
            throw new InvalidOperationException($"Invalid positive integer for {key} at line {lineNumber}: {value}");
        }

        return parsed;
    }

    private sealed class AiProfileBuilder
    {
        public string? Provider { get; set; }
        public string? Model { get; set; }
        public string? BaseUrl { get; set; }
        public string ApiKeySource { get; set; } = "none";
        public string? ApiKeyEnv { get; set; }
        public string? ApiKey { get; set; }
        public int? TimeoutSeconds { get; set; }

        public AiProfile Build(string name)
        {
            if (string.IsNullOrWhiteSpace(Provider))
            {
                throw new InvalidOperationException($"AI profile '{name}' requires provider.");
            }

            if (string.IsNullOrWhiteSpace(Model))
            {
                throw new InvalidOperationException($"AI profile '{name}' requires model.");
            }

            if (string.Equals(Provider, "openai-compatible", StringComparison.OrdinalIgnoreCase)
                && string.IsNullOrWhiteSpace(BaseUrl))
            {
                throw new InvalidOperationException($"AI profile '{name}' requires base_url.");
            }

            if (string.Equals(ApiKeySource, "none", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"AI profile '{name}' requires api_key or api_key_env.");
            }

            return new AiProfile(
                Provider.Trim(),
                Model.Trim(),
                Normalize(BaseUrl),
                ApiKeySource,
                Normalize(ApiKeyEnv),
                Normalize(ApiKey),
                TimeoutSeconds ?? DefaultTimeoutSeconds);
        }

        private static string? Normalize(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
