using TermBullet.Bootstrap;

var projectRoot = Directory.GetCurrentDirectory();

if (args.Length == 0)
{
    await TermBulletBootstrap.CreateTuiApp(projectRoot).RunAsync();
    return 0;
}

return await TermBulletBootstrap
    .CreateCliApp(projectRoot, Console.Out, Console.Error)
    .InvokeAsync(args);
