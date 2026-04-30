using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TankDestroyer.API;
using TankDestroyer.Engine;

namespace TankDestroyer.ConsoleApp;

public class ConsoleRenderer
{
    public void Render(
        GameTurn turn,
        World world,
        IReadOnlyDictionary<int, ConsoleColor> playerColors,
        IReadOnlyDictionary<int, string> playerLabels)
    {
        ClearConsole();

        for (int y = 0; y < world.Height; y++)
        {
            for (int x = 0; x < world.Width; x++)
            {
                var tank = turn.Tanks.FirstOrDefault(t => t.X == x && t.Y == y);
                if (tank != null)
                {
                    if (tank.Destroyed)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write("X ");
                    }
                    else
                    {
                        Console.ForegroundColor = playerColors.TryGetValue(tank.OwnerId, out var color)
                            ? color
                            : ConsoleColor.Gray;
                        Console.Write(GetTankChar(tank.TurretDirection) + " ");
                    }

                    continue;
                }

                var bullet = turn.Bullets.FirstOrDefault(b =>
                   (!b.Destroyed && b.X == x &&
                    b.Y == y) || (b.Destroyed && b.EndedAtX == x && b.EndedAtY == y));
                if (bullet != null)
                {
                    if (bullet.Explode)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                    }
                    else if (bullet.Destroyed)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                    }

                    Console.Write("* ");
                    continue;
                }

                var tile = world.GetTile(x, y);
                RenderTile(tile.TileType);
            }

            Console.WriteLine();
        }

        Console.ResetColor();
        Console.WriteLine($"Turn: {turn.Turn}");
        Console.WriteLine("Health:");
        foreach (var tank in turn.Tanks.OrderBy(t => t.OwnerId))
        {
            var label = playerLabels.TryGetValue(tank.OwnerId, out var playerName)
                ? playerName
                : $"Player {tank.OwnerId}";

            Console.ForegroundColor = playerColors.TryGetValue(tank.OwnerId, out var color)
                ? color
                : ConsoleColor.Gray;

            Console.Write($"- {label}");
            Console.ResetColor();
            Console.WriteLine($": {tank.Health} HP{(tank.Destroyed ? " (destroyed)" : string.Empty)}");
        }
        Console.WriteLine("Chat:");
        foreach (var message in world.Messages.Take(5))
        {
            var label = playerLabels.TryGetValue(message.OwnerId, out var playerName)
                ? playerName
                : $"Player {message.OwnerId}";

            Console.ForegroundColor = playerColors.TryGetValue(message.OwnerId, out var color)
                ? color
                : ConsoleColor.Gray;

            Console.WriteLine($"- {label}: {message.Text}");
        }
        Console.ResetColor();
    }

    private void RenderTile(TileType type)
    {
        switch (type)
        {
            case TileType.Grass:
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(". ");
                break;
            case TileType.Tree:
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.Write("T ");
                break;
            case TileType.Building:
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("# ");
                break;
            case TileType.Water:
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write("~ ");
                break;
            case TileType.Sand:
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(", ");
                break;
            default:
                Console.Write("  ");
                break;
        }
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

    private static void ClearConsole()
    {
        try
        {
            Console.Clear();
        }
        catch (IOException)
        {
            // Some hosts do not support full-screen clearing.
        }
    }
}
