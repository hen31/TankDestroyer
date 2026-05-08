using System.Reflection;
using System.Runtime.Loader;
using Spectre.Console;
using TankDestroyer.API;

namespace TankDestroyer.Engine.Services.Instantiate;

public class CollectBotsService: ICollectBotService
{
    public CollectBotsService()
    {
        AssemblyLoadContext.GetLoadContext(typeof(CollectBotsService).Assembly)?.Resolving += ResolveFindDll;
    }

    private  Assembly? ResolveFindDll(AssemblyLoadContext arg1, AssemblyName arg2)
    {
        AnsiConsole.MarkupLine($"Attempting to load {arg2.FullName}");
        var assembly = typeof(IPlayerBot).Assembly;
        return assembly;
    }


    public  Type[] LoadBots(string folder)
    {
        AnsiConsole.MarkupLine($"Loading bots from: [yellow]{folder}[/]");
        List<Type> allBots = new();
        var containingAssembly = typeof(IPlayerBot).Assembly;
        var typeOfPlayerBot = typeof(IPlayerBot);
        foreach (var dllFile in Directory.GetFiles(folder, "*.Bot.dll"))
        {
            AnsiConsole.MarkupLine($"Load from dll: [blue]{dllFile}[/]");
            try
            {
                var assembly = AssemblyLoadContext.GetLoadContext(typeof(CollectBotsService).Assembly)?.LoadFromAssemblyPath(dllFile);
                var botsInAssembly = assembly?.ExportedTypes.Where(c =>
                    c.IsAssignableTo(typeof(IPlayerBot)) && c.GetCustomAttribute<BotAttribute>() != null);
                if (botsInAssembly != null) allBots.AddRange(botsInAssembly);
            }
            catch (Exception e)
            {
                AnsiConsole.WriteException(e, ExceptionFormats.ShortenEverything);
                throw;
            }
        }

        return allBots.ToArray();
    }
}