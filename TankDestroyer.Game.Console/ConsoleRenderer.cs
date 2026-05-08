using System.Text;
using Spectre.Console;
using TankDestroyer.API;
using TankDestroyer.ConsoleApp.Extensions;
using TankDestroyer.Engine;

namespace TankDestroyer.Console;

public class ConsoleRenderer
{
    public async Task AnimateTurn(
        GameTurn turn,
        GameTurn? previousTurn,
        World world,
        IReadOnlyDictionary<int, Color> playerColors,
        IReadOnlyDictionary<int, string> playerLabels,
        int durationMs = 500)
    {
        var frames = 10;
        var frameDelay = durationMs / frames;

        // Ensure we clear before starting the live display
        AnsiConsole.Clear();

        await AnsiConsole.Live(new Markup(GenerateFullFrame(turn, previousTurn, world, playerColors, playerLabels, 0.0f)))
            .StartAsync(async ctx =>
            {
                for (int i = 1; i <= frames; i++)
                {
                    float progress = i / (float)frames;
                    ctx.UpdateTarget(new Markup(GenerateFullFrame(turn, previousTurn, world, playerColors, playerLabels, progress)));
                    await Task.Delay(frameDelay);
                }
            });
    }

    public void Render(
        GameTurn turn,
        GameTurn? previousTurn,
        World world,
        IReadOnlyDictionary<int, Color> playerColors,
        IReadOnlyDictionary<int, string> playerLabels)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Markup(GenerateFullFrame(turn, previousTurn, world, playerColors, playerLabels, 1.0f)));
    }

    private string GenerateFullFrame(GameTurn turn, GameTurn? previousTurn, World world, IReadOnlyDictionary<int, Color> playerColors, IReadOnlyDictionary<int, string> playerLabels, float bulletProgress)
    {
        var sb = new StringBuilder();
        sb.Append(GenerateGrid(turn, previousTurn, world, playerColors, bulletProgress));
        sb.Append(GenerateStats(turn, playerColors, playerLabels));
        return sb.ToString();
    }

    private string GenerateGrid(GameTurn turn, GameTurn? previousTurn, World world, IReadOnlyDictionary<int, Color> playerColors, float bulletProgress)
    {
        var sb = new StringBuilder();
        
        // Calculate interpolated bullet positions for this frame
        var currentBullets = new List<(int X, int Y, int OwnerId, bool Explode, bool Destroyed)>();
        foreach (var b in turn.Bullets)
        {
            // Find this bullet in the previous turn to know where it started THIS turn.
            // If not found, it was just fired, so it starts at b.StartingX/Y (tank position).
            var prevBullet = previousTurn?.Bullets.FirstOrDefault(pb => pb.Id == b.Id);
            
            int startX = prevBullet?.X ?? b.StartingX;
            int startY = prevBullet?.Y ?? b.StartingY;
            int endX = b.Destroyed ? b.EndedAtX : b.X;
            int endY = b.Destroyed ? b.EndedAtY : b.Y;

            // Simple linear interpolation
            int curX = (int)Math.Round(startX + (endX - startX) * bulletProgress);
            int curY = (int)Math.Round(startY + (endY - startY) * bulletProgress);
            
            // Only show explosion on final progress if it exploded
            bool isExploding = b.Explode && bulletProgress >= 0.8f;
            
            currentBullets.Add((curX, curY, b.OwnerId, isExploding, b.Destroyed));
        }

        for (int y = 0; y < world.Height; y++)
        {
            for (int x = 0; x < world.Width; x++)
            {
                // Draw Tanks
                var tank = turn.Tanks.FirstOrDefault(t => t.X == x && t.Y == y);
                if (tank != null)
                {
                    if (tank.Destroyed)
                    {
                        sb.Append("[grey]X [/]");
                    }
                    else
                    {
                        var color = playerColors.TryGetValue(tank.OwnerId, out var c) ? c : Color.Grey;
                        sb.Append($"[{color.ToMarkup()}]{GetTankChar(tank.TurretDirection)} [/]");
                    }
                    continue;
                }

                // Draw Bullets (Interpolated)
                var bullet = currentBullets.FirstOrDefault(b => b.X == x && b.Y == y);
                if (bullet != default)
                {
                    if (bullet.Explode)
                    {
                        sb.Append("[red]* [/]");
                    }
                    else if (bullet.Destroyed && bulletProgress >= 1.0f)
                    {
                        sb.Append("[grey]* [/]");
                    }
                    else if (!bullet.Destroyed || bulletProgress < 1.0f)
                    {
                        var color = playerColors.TryGetValue(bullet.OwnerId, out var c) ? c : Color.White;
                        sb.Append($"[{color.ToMarkup()}]* [/]");
                    }
                    else
                    {
                         sb.Append(GetTileMarkup(world.GetTile(x, y).TileType));
                    }
                    continue;
                }
                
                
                var ammoBox = turn.MunitionBoxes.FirstOrDefault(b => b.X == x && b.Y == y);
                if (ammoBox != null)
                {
                    var color = Color.RosyBrown;
                    sb.Append($"[{color.ToMarkup()}]{ammoBox.GetAscii()} [/]");
                    continue;
                }

                var tile = world.GetTile(x, y);
                sb.Append(GetTileMarkup(tile.TileType));
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }

    private string GenerateStats(GameTurn turn, IReadOnlyDictionary<int, Color> playerColors, IReadOnlyDictionary<int, string> playerLabels)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"\nTurn: [yellow]{turn.Turn}[/]");
        sb.AppendLine("Health:");
        foreach (var tank in turn.Tanks.OrderBy(t => t.OwnerId))
        {
            var label = playerLabels.TryGetValue(tank.OwnerId, out var playerName)
                ? playerName
                : $"Player {tank.OwnerId}";

            var color = playerColors.TryGetValue(tank.OwnerId, out var c) ? c : Color.Grey;
            var destroyedInfo = tank.Destroyed ? " [red](destroyed)[/]" : string.Empty;
            
            sb.AppendLine($" - [{color.ToMarkup()}]{Markup.Escape(label)}[/]: {tank.Health} HP{destroyedInfo}");
        }
        return sb.ToString();
    }

    private string GetTileMarkup(TileType type)
    {
        return type switch
        {
            TileType.Grass => "[green]. [/]",
            TileType.Tree => "[darkgreen]T [/]",
            TileType.Building => "[olive]# [/]",
            TileType.Water => "[blue]~ [/]",
            TileType.Sand => "[yellow], [/]",
            _ => "  "
        };
    }

    private char GetTankChar(TurretDirection direction)
    {
        return direction switch
        {
            TurretDirection.North => '↓',
            TurretDirection.South => '↑',
            TurretDirection.East => '←',
            TurretDirection.West => '→',
            TurretDirection.NorthEast => '↙',
            TurretDirection.NorthWest => '↘',
            TurretDirection.SouthEast => '↖',
            TurretDirection.SouthWest => '↗',
            _ => 'O'
        };
    }
}
