using Godot;
using TankDestroyer.API;

namespace TankDestroyer.Extensions;

public static class TurretDirectionExtensions
{
    public static Vector3 Get3DVector(this TurretDirection turretDirection)
    {
        return turretDirection switch
        {
            TurretDirection.South =>    new Vector3(0, 0f,    0),
            TurretDirection.SouthEast => new Vector3(0, 45f,   0),
            TurretDirection.East =>     new Vector3(0, 90f,   0),
            TurretDirection.NorthEast => new Vector3(0, 135f,  0),
            TurretDirection.North =>    new Vector3(0, 180f,  0),
            TurretDirection.NorthWest => new Vector3(0, 225f,  0),
            TurretDirection.West =>     new Vector3(0, 270f,  0),
            TurretDirection.SouthWest => new Vector3(0, 315f,  0),
            _ => new Vector3(0, 0, 0)
        };
    }
}