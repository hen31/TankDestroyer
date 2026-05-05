using TankDestroyer.API;
using TankDestroyer.Engine.Objects;

namespace TankDestroyer.Engine.Services.Ammo;

public class AmmoService(Game game) : IAmmoService
{
    private readonly Game _game = game;

    public int SpawnAmmo(int range)
    {
        var nonDestroyedTanks = _game.Tanks.Where(tank => !tank.Destroyed).ToList();
        
        if (_game.MunitionBoxes.Count >= nonDestroyedTanks.Count)
        {
            return _game.MunitionBoxes.Count;
        }

        var averageAmmoDepletion = (int)Math.Ceiling(nonDestroyedTanks.Select(t => 10 - t.Ammo).Average());

        var even = range % 2 == 0;
        if (!even)
        {
            range = (range + 1);
        }

        foreach (var gameTank in _game.Tanks)
        {
            if (gameTank.Destroyed) continue;

            var spawnXRange = Enumerable.Range(gameTank.X - range / 2, gameTank.X + range / 2).ToList();
            var spawnYRange = Enumerable.Range(gameTank.Y - range / 2, gameTank.Y + range / 2).ToList();
            var possibleSpawns = new List<Location>();

            for (var x = 0; x < spawnXRange.Count; x++)
            {
                for (var y = 0; y < spawnYRange.Count; y++)
                {
                    var location = new Location(x, y);
                    if (!IsLocationIllegal(location))
                    {
                        possibleSpawns.Add(location);
                    }
                }
            }

            var random = new Random();
            var ammoLocation = possibleSpawns.ElementAt(random.Next(0, possibleSpawns.Count));
            _game.MunitionBoxes.Add(new MunitionBox(ammoLocation.X, ammoLocation.Y, averageAmmoDepletion));
        }
        
        return _game.MunitionBoxes.Count;
    }

    private bool IsLocationIllegal(Location location)
    {
        if (location.X < 0 || location.Y < 0 || location.X >= _game.World.Width || location.Y >= _game.World.Height)
        {
            return true;
        }


        var tile = _game.World.GetTile(location.X, location.Y);

        if (tile.TileType is TileType.Building or TileType.Water or TileType.Tree)
        {
            return true;
        }

        foreach (var gameTank in _game.Tanks)
        {
            var gameTankIllegalXRange = Enumerable.Range(gameTank.X - 1, 3).ToList();
            var gameTankIllegalYRange = Enumerable.Range(gameTank.Y - 1, 3).ToList();

            for (var x = 0; x < gameTankIllegalXRange.Count; x++)
            {
                for (var y = 0; y < gameTankIllegalYRange.Count; x++)
                {
                    if (location.X == x && location.Y == y)
                    {
                        return true;
                    }
                }
            }
        }


        return false;
    }
}