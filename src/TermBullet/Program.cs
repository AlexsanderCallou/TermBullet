using TermBullet.Bootstrap;
using TermBullet.Services.Configuration;

if (TermBulletBootstrap.IsInformationalCliRequest(args))
{
    return await TermBulletBootstrap
        .CreateInformationalCliApp(Console.Out, Console.Error)
        .InvokeAsync(args);
}

var setupService = new DataDirectorySetupService(
    new TermBulletConfigService(AppContext.BaseDirectory),
    new DataDirectoryValidator());

TermBulletRuntimePaths runtimePaths;
try
{
    runtimePaths = await setupService.ResolveOrPromptAsync(Console.In, Console.Out);
}
catch (Exception exception)
{
    await Console.Error.WriteLineAsync(exception.Message);
    return 1;
}

if (args.Length == 0)
{
    await TermBulletBootstrap.CreateTuiApp(runtimePaths).RunAsync();
    return 0;
}

return await TermBulletBootstrap
    .CreateCliApp(runtimePaths, Console.Out, Console.Error)
    .InvokeAsync(args);
