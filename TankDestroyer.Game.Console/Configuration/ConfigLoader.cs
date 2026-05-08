using Spectre.Console;
using TankDestroyer.Engine.Services.Instantiate;

namespace TankDestroyer.Console.Configuration;

public class ConfigLoader(ICollectBotService collectBotsService, ICollectMapsService collectMapsService) : IConfigLoader
{
    private readonly ICollectBotService _collectBotService = collectBotsService;
    private readonly ICollectMapsService _collectMapsService = collectMapsService;

    public InitialGameObject? LoadConfig()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine($"[blue]Loading application[/]");
        
        var botFolder = ResolvePath("..\\Build\\Bots", "..\\Bots");
        var mapFolder = ResolvePath("..\\Maps", "..\\Maps");

        LoadGameConfig();
        if (!Directory.Exists(botFolder))
        {
            AnsiConsole.MarkupLine($"[red]Bot folder not found:[/] {botFolder}");
            return null;
        }

        if (!Directory.Exists(mapFolder))
        {
            AnsiConsole.MarkupLine($"[red]Map folder not found:[/] {mapFolder}");
            return null;
        }

        var botTypes = _collectBotService.LoadBots(botFolder);
        if (botTypes.Length == 0)
        {
            AnsiConsole.MarkupLine($"[red]No bots found in:[/] {botFolder}");
            return null;
        }

        var maps = _collectMapsService.LoadMaps(mapFolder);
        if (maps.Length == 0)
        {
            AnsiConsole.MarkupLine($"[red]No maps found in:[/] {mapFolder}");
            return null;
        }
        
        return new InitialGameObject(){ Worlds = maps, Bots = botTypes };
    }
    
    private GameConfig LoadGameConfig()
    {
        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
        if (!File.Exists(configPath))
        {
            return new GameConfig();
        }

        var json = File.ReadAllText(configPath);
        return System.Text.Json.JsonSerializer.Deserialize<GameConfig>(json) ?? new GameConfig();
    }

    private string ResolvePath(string? configuredPath, string fallback)
    {
        var value = string.IsNullOrWhiteSpace(configuredPath) ? fallback : configuredPath;
        value = value.Replace('\\', Path.DirectorySeparatorChar);
        var path = Path.IsPathRooted(value)
            ? value
            : Path.GetFullPath(value);
        return path;
    }
}