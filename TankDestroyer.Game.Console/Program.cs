using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TankDestroyer.Console;
using TankDestroyer.Console.Configuration;
using TankDestroyer.Engine.Services.Instantiate;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices(services =>
{
    services.AddTransient<IConfigLoader, ConfigLoader>();
    services.AddTransient<ICollectBotService, CollectBotsService>();
    services.AddTransient<ICollectMapsService, CollectMapsService>();
    services.AddTransient<IApp,App>();
});

var host = builder.Build();

await host.Services.GetRequiredService<IApp>().RunAsync();