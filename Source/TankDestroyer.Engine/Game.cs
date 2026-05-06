using TankDestroyer.API;
using TankDestroyer.Engine.Objects;

namespace TankDestroyer.Engine;

public class Game
{
    public Game(World world, IPlayerBot[] playerBots)
    {
        World = world;
        Players = new PlayerBot[playerBots.Length];
        Tanks = new Tank[playerBots.Length];
        for (int i = 0; i < playerBots.Length; i++)
        {
            Players[i] = new PlayerBot(playerBots[i], i);
            Tanks[i] = new Tank(i);
            var spawnPoint = World.SpawnPoints[i];
            Tanks[i].X = (int)spawnPoint.X;
            Tanks[i].Y = (int)spawnPoint.Y;
            Tanks[i].Ammo = 10;
            Tanks[i].MaxAmmo = 10;
        }
    }

    public PlayerBot[] Players { get; set; }
    public World World { get; set; }
    public Tank[] Tanks { get; set; }

    public List<GameTurn> Turns { get; set; } = new();

    public List<Bullet> Bullets { get; set; } = new();
    public List<MunitionBox> MunitionBoxes { get; set; } = [];
}