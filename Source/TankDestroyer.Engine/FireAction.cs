using TankDestroyer.API;

namespace TankDestroyer.Engine;

public class FireAction : TankAction
{
    public FireAction(int playerId) : base(playerId)
    {
        Priority = 1000;
    }

    internal override bool Execute(Game game)
    {
        var tank = game.Tanks.FirstOrDefault(c => c.OwnerId == OwnerId);
        if (tank == null)
        {
            return false;
        }

        if (tank.Ammo == 0)
        {
            return false;
        }

        Bullet bullet = new(tank.OwnerId)
        {
            X = tank.X,
            Y = tank.Y,
            StartingX = tank.X,
            StartingY = tank.Y,
            Direction = tank.TurretDirection,
        };
        tank.Fired = true;
        game.Bullets.Add(bullet);
        tank.Ammo--;
        return true;
    }
}