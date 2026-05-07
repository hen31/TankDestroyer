using TankDestroyer.API;
using TankDestroyer.Engine.Objects;

namespace TankDestroyer.Engine.Services.Ammo;

public class AmmoService(Game game) : IAmmoService
{
    private readonly Game _game = game;

    public int SpawnAmmo(int range)
    {
        var nonDestroyedTanks = _game.Tanks.Where(tank => !tank.Destroyed).ToList();
     
        var averageAmmo = nonDestroyedTanks.Select(t => t.Ammo).Average();
        var maxBoxes = nonDestroyedTanks.Count; 
        
        if (averageAmmo > 5 || _game.MunitionBoxes.Count >= maxBoxes) return _game.MunitionBoxes.Count;
  
        var even = range % 2 == 0;
        if (!even)
        {
            range = (range + 1);
        }

        foreach (var gameTank in nonDestroyedTanks.OrderByDescending(DistanceToClosestAmmo))
        {
            if (gameTank.Destroyed) continue;

            var spawnXRange = Enumerable.Range(gameTank.X - range / 2, range + 1)
                .Where(n => n >= 0 && n < _game.World.Width)
                .ToList();
            var spawnYRange = Enumerable.Range(gameTank.Y - range / 2, range + 1)
                .Where(n => n >= 0 && n < _game.World.Height)
                .ToList();
            var possibleSpawns = new List<Location>();

            for (var x = 0; x < spawnXRange.Count; x++)
            {
                for (var y = 0; y < spawnYRange.Count; y++)
                {
                    var location = new Location(spawnXRange.ElementAt(x), spawnYRange.ElementAt(y));
                    if (!IsLocationIllegal(location))
                    {
                        possibleSpawns.Add(location);
                    }
                }
            }
            
            if (possibleSpawns.Count == 0) continue;

            var random = new Random();
            var ammoLocation = possibleSpawns.ElementAt(random.Next(0, possibleSpawns.Count));
            _game.MunitionBoxes.Add(new MunitionBox(ammoLocation.X, ammoLocation.Y));

            if (_game.MunitionBoxes.Count >= nonDestroyedTanks.Count)
            {
                return _game.MunitionBoxes.Count;
            }
        }

        return _game.MunitionBoxes.Count;
    }

    public void PickupAmmo(GameTurn turn)
    {
        var ammoBoxes = turn.MunitionBoxes;

        foreach (var tank in turn.Tanks.Where(tank => !tank.Destroyed).OrderBy(tank => tank.Ammo))
        {
            var ammo = ammoBoxes
                .Where(ammo => !ammo.IsPickedUp)
                .FirstOrDefault(ammo => ammo.X == tank.X && ammo.Y == tank.Y);
            
            if (ammo == null) continue;
            
            var realTank = _game.Tanks.FirstOrDefault(t => t.OwnerId == tank.OwnerId);
            if (realTank == null) continue;
            
            ammo.PickUpBy(realTank);
           
            var original = _game.MunitionBoxes.FirstOrDefault(m => m.Id == ammo.Id);
            if (original != null)
            {
                _game.MunitionBoxes.Remove(original);
            }
        }
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

        if (_game.MunitionBoxes.Any(ammo => ammo.X == location.X && ammo.Y == location.Y))
        {
            return true;
        }

        foreach (var gameTank in _game.Tanks)
        {
            var gameTankIllegalXRange = Enumerable.Range(gameTank.X - 1, 3).ToList();
            var gameTankIllegalYRange = Enumerable.Range(gameTank.Y - 1, 3).ToList();

            for (var x = 0; x < gameTankIllegalXRange.Count; x++)
            {
                for (var y = 0; y < gameTankIllegalYRange.Count; y++)
                {
                    if (location.X == gameTankIllegalXRange.ElementAt(x) && location.Y == gameTankIllegalYRange.ElementAt(y))
                    {
                        return true;
                    }
                }
            }
        }


        return false;
    }

    private int DistanceToClosestAmmo(Tank tank)
    {
        var closest = int.MaxValue;

        foreach (var ammo in _game.MunitionBoxes)
        {
            var location = Math.Abs(ammo.X - tank.X) + Math.Abs(ammo.Y - tank.Y);

            if (location <= closest)
            {
                closest = location;
            }
        }

        return closest;
    }
}