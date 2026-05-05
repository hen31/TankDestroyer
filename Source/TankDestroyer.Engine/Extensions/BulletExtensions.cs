using System.Numerics;
using TankDestroyer.API;

namespace TankDestroyer.Engine.Extensions;

public static class BulletExtensions
{
    public static Vector2 GetVector(this Bullet bullet)
    {
        var direction = new Vector2(0, 0);
        if (bullet.Direction.HasFlag(TurretDirection.North))
        {
            direction += new Vector2(0, -1);
        }

        if (bullet.Direction.HasFlag(TurretDirection.South))
        {
            direction += new Vector2(0, 1);
        }

        if (bullet.Direction.HasFlag(TurretDirection.West))
        {
            direction += new Vector2(-1, 0);
        }

        if (bullet.Direction.HasFlag(TurretDirection.East))
        {
            direction += new Vector2(1, 0);
        }
        
        return direction;
    }
}