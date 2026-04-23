using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using TermBullet.Application.Ports;
using TermBullet.Infrastructure.Persistence.JsonFiles;

namespace TermBullet.Infrastructure.Export;

public sealed class JsonDataTransferService(
    string projectRootPath,
    SafeJsonFileStore fileStore,
    LocalJsonIndexService indexService) : IDataTransferService
{
    private const string ExportFormat = "termbullet-export";
    private const int ExportVersion = 1;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly Regex MonthlyRelativePathPattern = new(
        "^data/[0-9]{4}/data_[0-9]{2}_[0-9]{4}\\.json$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public async Task ExportAsync(string outputPath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        var package = new ExportPackage
        {
            ExportedAt = DateTimeOffset.UtcNow,
            MonthlyFiles = await ReadMonthlyFilesAsync(cancellationToken),
            Settings = await ReadSettingsAsync(cancellationToken)
        };

        var json = JsonSerializer.Serialize(package, JsonOptions);
        await WriteExportPackageAsync(outputPath, json, cancellationToken);
    }

    public async Task ImportAsync(string inputPath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);

        if (!File.Exists(inputPath))
        {
            throw new FileNotFoundException("Import file not found.", inputPath);
        }

        var package = await ReadPackageAsync(inputPath, cancellationToken);
        ValidatePackage(package);

        foreach (var monthlyFile in package.MonthlyFiles)
        {
            var targetPath = ResolveMonthlyTargetPath(monthlyFile.RelativePath);
            var backupPath = GetMonthlyBackupPath(targetPath);
            var json = JsonSerializer.Serialize(monthlyFile.Content, JsonOptions);
            await fileStore.WriteAsync(targetPath, backupPath, json, cancellationToken);
        }

        if (package.Settings is not null)
        {
            var settingsPath = GetSettingsPath();
            var backupPath = GetSettingsBackupPath();
            var json = JsonSerializer.Serialize(package.Settings, JsonOptions);
            await fileStore.WriteAsync(settingsPath, backupPath, json, cancellationToken);
        }

        await indexService.RebuildAsync(cancellationToken);
    }

    private async Task<List<ExportedMonthlyFile>> ReadMonthlyFilesAsync(CancellationToken cancellationToken)
    {
        var dataRootPath = Path.Combine(projectRootPath, "data");
        if (!Directory.Exists(dataRootPath))
        {
            return [];
        }

        var monthlyFilePaths = Directory
            .EnumerateFiles(dataRootPath, "data_??_????.json", SearchOption.AllDirectories)
            .Where(path => !path.EndsWith(".backup.json", StringComparison.OrdinalIgnoreCase))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var monthlyFiles = new List<ExportedMonthlyFile>(monthlyFilePaths.Length);
        foreach (var path in monthlyFilePaths)
        {
            var json = await File.ReadAllTextAsync(path, cancellationToken);
            monthlyFiles.Add(new ExportedMonthlyFile
            {
                RelativePath = NormalizeRelativePath(Path.GetRelativePath(projectRootPath, path)),
                Period = ReadPeriod(json),
                Content = ParseJsonElement(json)
            });
        }

        return monthlyFiles;
    }

    private async Task<JsonElement?> ReadSettingsAsync(CancellationToken cancellationToken)
    {
        var settingsPath = GetSettingsPath();
        if (!File.Exists(settingsPath))
        {
            return null;
        }

        var json = await fileStore.ReadOrRecoverAsync(settingsPath, GetSettingsBackupPath(), cancellationToken);
        return ParseJsonElement(json);
    }

    private static string ReadPeriod(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.TryGetProperty("period", out var periodElement)
            ? periodElement.GetString() ?? string.Empty
            : string.Empty;
    }

    private static JsonElement ParseJsonElement(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.Clone();
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException("Invalid JSON content.", exception);
        }
    }

    private static async Task WriteExportPackageAsync(
        string outputPath,
        string json,
        CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(outputPath);
        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new ArgumentException("Output path must include a directory.", nameof(outputPath));
        }

        Directory.CreateDirectory(directory);
        var tempPath = $"{outputPath}.{Guid.NewGuid():N}.tmp";
        await File.WriteAllTextAsync(tempPath, json, cancellationToken);

        try
        {
            if (File.Exists(outputPath))
            {
                File.Replace(tempPath, outputPath, destinationBackupFileName: null, ignoreMetadataErrors: true);
            }
            else
            {
                File.Move(tempPath, outputPath);
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

    private static async Task<ExportPackage> ReadPackageAsync(string inputPath, CancellationToken cancellationToken)
    {
        try
        {
            var json = await File.ReadAllTextAsync(inputPath, cancellationToken);
            return JsonSerializer.Deserialize<ExportPackage>(json, JsonOptions)
                ?? throw new InvalidDataException("Import package could not be deserialized.");
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException("Import file contains malformed JSON.", exception);
        }
    }

    private static void ValidatePackage(ExportPackage package)
    {
        if (!string.Equals(package.Format, ExportFormat, StringComparison.Ordinal))
        {
            throw new InvalidDataException($"Unsupported export package format: {package.Format}.");
        }

        if (package.Version != ExportVersion)
        {
            throw new InvalidDataException($"Unsupported export package version: {package.Version}.");
        }

        var ids = new HashSet<Guid>();
        var refsByPeriod = new HashSet<string>(StringComparer.Ordinal);

        foreach (var monthlyFile in package.MonthlyFiles)
        {
            ValidateMonthlyRelativePath(monthlyFile.RelativePath);

            if (monthlyFile.Content.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidDataException($"Monthly file content must be a JSON object: {monthlyFile.RelativePath}.");
            }

            if (!monthlyFile.Content.TryGetProperty("items", out var itemsElement) || itemsElement.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidDataException($"Monthly file is missing a valid items array: {monthlyFile.RelativePath}.");
            }

            var period = GetRequiredString(monthlyFile.Content, "period", monthlyFile.RelativePath);
            foreach (var item in itemsElement.EnumerateArray())
            {
                var id = GetRequiredGuid(item, "id", monthlyFile.RelativePath);
                if (!ids.Add(id))
                {
                    throw new InvalidOperationException($"Duplicate internal ID detected during import: {id}.");
                }

                var publicRef = GetRequiredString(item, "public_ref", monthlyFile.RelativePath);
                var refKey = $"{period}:{publicRef}";
                if (!refsByPeriod.Add(refKey))
                {
                    throw new InvalidOperationException($"Duplicate public ref detected during import: {publicRef} in {period}.");
                }

                _ = GetRequiredString(item, "type", monthlyFile.RelativePath);
                _ = GetRequiredString(item, "content", monthlyFile.RelativePath);
                _ = GetRequiredString(item, "status", monthlyFile.RelativePath);
                _ = GetRequiredString(item, "collection", monthlyFile.RelativePath);
                _ = GetRequiredString(item, "priority", monthlyFile.RelativePath);
                _ = GetRequiredString(item, "created_at", monthlyFile.RelativePath);
                _ = GetRequiredString(item, "updated_at", monthlyFile.RelativePath);
            }
        }
    }

    private string ResolveMonthlyTargetPath(string relativePath)
    {
        ValidateMonthlyRelativePath(relativePath);
        var parts = relativePath.Split('/');
        return Path.Combine(projectRootPath, Path.Combine(parts));
    }

    private static void ValidateMonthlyRelativePath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || !MonthlyRelativePathPattern.IsMatch(relativePath))
        {
            throw new InvalidDataException($"Unsupported monthly file path in import package: {relativePath}.");
        }
    }

    private static string GetRequiredString(JsonElement element, string propertyName, string source)
    {
        if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.String)
        {
            throw new InvalidDataException($"Missing required string property '{propertyName}' in {source}.");
        }

        var value = property.GetString();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidDataException($"Empty required string property '{propertyName}' in {source}.");
        }

        return value;
    }

    private static Guid GetRequiredGuid(JsonElement element, string propertyName, string source)
    {
        var value = GetRequiredString(element, propertyName, source);
        return Guid.TryParse(value, out var id)
            ? id
            : throw new InvalidDataException($"Invalid GUID property '{propertyName}' in {source}.");
    }

    private string GetSettingsPath() => Path.Combine(projectRootPath, "data", "settings.json");

    private string GetSettingsBackupPath() => Path.Combine(projectRootPath, "data", "settings.backup.json");

    private static string GetMonthlyBackupPath(string monthlyPath)
    {
        var directory = Path.GetDirectoryName(monthlyPath)
            ?? throw new InvalidOperationException("Monthly path must include a directory.");
        var fileName = Path.GetFileNameWithoutExtension(monthlyPath);
        return Path.Combine(directory, $"{fileName}.backup.json");
    }

    private static string NormalizeRelativePath(string path) => path.Replace('\\', '/');

    private sealed class ExportPackage
    {
        [JsonPropertyName("format")]
        public string Format { get; set; } = ExportFormat;

        [JsonPropertyName("version")]
        public int Version { get; set; } = ExportVersion;

        [JsonPropertyName("exported_at")]
        public DateTimeOffset ExportedAt { get; set; }

        [JsonPropertyName("monthly_files")]
        public List<ExportedMonthlyFile> MonthlyFiles { get; set; } = [];

        [JsonPropertyName("settings")]
        public JsonElement? Settings { get; set; }
    }

    private sealed class ExportedMonthlyFile
    {
        [JsonPropertyName("relative_path")]
        public string RelativePath { get; set; } = string.Empty;

        [JsonPropertyName("period")]
        public string Period { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public JsonElement Content { get; set; }
    }
}
