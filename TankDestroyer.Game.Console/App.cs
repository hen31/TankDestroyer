using System.Collections.Immutable;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Spectre.Console;
using TankDestroyer.API;
using TankDestroyer.Console.Configuration;
using TankDestroyer.Engine;

namespace TankDestroyer.Console;

public class App(IConfigLoader loader) : IApp
{
    private readonly IConfigLoader _loader = loader;

    public async Task RunAsync()
    {
        System.Console.OutputEncoding = Encoding.UTF8;
        AnsiConsole.Cursor.Hide();

        try
        {
            AnsiConsole.MarkupLine($"\n\n[Bold]Welcome to[/]");
            AnsiConsole.Write(new FigletText("Tank Destroyer!"));
            await Task.Delay(2000);

            var initialConfig = _loader.LoadConfig();
            if (initialConfig == null) return;
            
            var selectedMap = SelectMap(initialConfig.Worlds);
            var selectedBotTypes = SelectBots(initialConfig.Bots, selectedMap.SpawnPoints.Length);

            var bots = selectedBotTypes
                .Select(type => (IPlayerBot)Activator.CreateInstance(type)!)
                .ToArray();

            var playerColors = new Dictionary<int, Color>();
            var playerLabels = new Dictionary<int, string>();
            for (var i = 0; i < selectedBotTypes.Count; i++)
            {
                var attribute = selectedBotTypes[i].GetCustomAttribute<BotAttribute>();
                var colorHex = attribute?.Color ?? "#808080";

                if (!Color.TryFromHex(colorHex, out var color))
                {
                    color = Color.Grey;
                }

                playerColors[i] = color;
                playerLabels[i] = attribute?.Name ?? selectedBotTypes[i].Name;
            }

            var runner = new GameRunner(selectedMap, bots);
            var renderer = new ConsoleRenderer();

            GameTurn? previousTurn = null;
            var initialTurn = runner.GetTurns().Last();
            await renderer.AnimateTurn(initialTurn, null, selectedMap, playerColors, playerLabels);
            previousTurn = initialTurn;

            while (!runner.Finished)
            {
                var turnsToPlay = AskTurnsToPlay();
                if (turnsToPlay <= 0)
                {
                    break;
                }

                for (var turnIndex = 0; turnIndex < turnsToPlay && !runner.Finished; turnIndex++)
                {
                    runner.DoTurn();
                    var lastTurn = runner.GetTurns().Last();
                    await renderer.AnimateTurn(lastTurn, previousTurn, selectedMap, playerColors, playerLabels);
                    previousTurn = lastTurn;

                    if (turnsToPlay > 1)
                    {
                        await Task.Delay(500);
                    }
                }
            }

            AnsiConsole.MarkupLine("[bold green]Game Finished![/]");
        }
        finally
        {
            AnsiConsole.Cursor.Show();
        }
    }

    private World SelectMap(IReadOnlyList<World> maps)
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<World>()
                .Title("Select [green]map[/]:")
                .PageSize(10)
                .AddChoices(maps)
                .UseConverter(map => $"{map.Name} ({map.Width}x{map.Height}) spawns: {map.SpawnPoints.Length}"));
    }

    private List<Type> SelectBots(IReadOnlyList<Type> botTypes, int maxBots)
    {
        var selectedTypes = AnsiConsole.Prompt(
            new MultiSelectionPrompt<Type>()
                .Title($"Select [green]bots[/] (max {maxBots}):")
                .PageSize(100)
                .Required()
                .InstructionsText(
                    "[grey](Press [blue]<space>[/] to toggle a bot, " +
                    "[green]<enter>[/] to accept)[/]")
                .AddChoices(botTypes)
                .UseConverter(type =>
                {
                    var attribute = type.GetCustomAttribute<BotAttribute>();
                    var name = Markup.Escape(attribute?.Name ?? type.Name);
                    var creator = Markup.Escape(attribute?.Creator ?? "Unknown");

                    var colorCandidate = attribute?.Color;

                    if (string.IsNullOrWhiteSpace(colorCandidate) || !Color.TryFromHex(colorCandidate, out _))
                    {
                        colorCandidate = GetDeterministicHexColor(name);
                    }

                    return $"[#{colorCandidate}]{name}[/] by {creator}";
                }));

        if (selectedTypes.Count > maxBots)
        {
            AnsiConsole.MarkupLine($"[yellow]Warning:[/] Too many bots selected. Using first {maxBots}.");
            return selectedTypes.Take(maxBots).ToList();
        }

        return selectedTypes;
    }

    private string GetDeterministicHexColor(string input)
    {
        var hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(input));
        // Use the first 3 bytes to create a valid RRGGBB hex color
        return $"{hashBytes[0]:X2}{hashBytes[1]:X2}{hashBytes[2]:X2}";
    }

    private int AskTurnsToPlay()
    {
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("How many [green]turns[/] to play?")
                .AddChoices(new[] { "1 turn", "X turns", "All remaining", "Quit" }));

        switch (choice)
        {
            case "1 turn":
                return 1;
            case "X turns":
                return AnsiConsole.Ask<int>("Number of turns:");
            case "All remaining":
                return int.MaxValue;
            case "Quit":
            default:
                return 0;
        }
    }
}