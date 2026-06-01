namespace TermBullet.Services.Configuration;

public sealed class DataDirectorySetupService(
    TermBulletConfigService configService,
    DataDirectoryValidator validator,
    Func<string>? defaultDataRootFactory = null)
{
    private readonly Func<string> _defaultDataRootFactory =
        defaultDataRootFactory ?? GetDefaultDataRoot;

    public async Task<TermBulletRuntimePaths> ResolveOrPromptAsync(
        TextReader input,
        TextWriter output,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);

        var config = await configService.LoadAsync(cancellationToken);
        if (config is not null)
        {
            return BuildRuntimePaths(config.DataRoot);
        }

        var defaultRoot = _defaultDataRootFactory();
        await output.WriteLineAsync("TermBullet needs a local data directory.");
        await output.WriteLineAsync($"Config will be saved at: {configService.ConfigPath}");
        await output.WriteLineAsync($"Default: {defaultRoot}");
        await output.WriteAsync("Choose TermBullet data directory (press Enter for default): ");

        var selected = await input.ReadLineAsync(cancellationToken);
        var dataRoot = string.IsNullOrWhiteSpace(selected) ? defaultRoot : selected;
        var paths = BuildRuntimePaths(dataRoot);
        await configService.SaveAsync(new TermBulletConfig(paths.DataRoot), cancellationToken);
        await output.WriteLineAsync($"TermBullet data directory configured: {paths.DataRoot}");
        return paths;
    }

    private TermBulletRuntimePaths BuildRuntimePaths(string dataRoot)
    {
        var validatedRoot = validator.ValidateAndPrepare(dataRoot);
        return new TermBulletRuntimePaths(
            configService.ConfigPath,
            validatedRoot,
            Path.Combine(validatedRoot, "data"));
    }

    private static string GetDefaultDataRoot()
    {
        var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var root = string.IsNullOrWhiteSpace(documents)
            ? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
            : documents;

        if (string.IsNullOrWhiteSpace(root))
        {
            root = AppContext.BaseDirectory;
        }

        return Path.Combine(root, "TermBullet");
    }
}
