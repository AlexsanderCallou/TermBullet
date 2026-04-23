using TermBullet.Bootstrap;

return await TermBulletBootstrap
    .CreateCliApp(Directory.GetCurrentDirectory(), Console.Out, Console.Error)
    .InvokeAsync(args);
