using TankDestroyer.API;

namespace DOA.Bot;

public record Target
{
    public ITank Tank { get; init; }

    public double Distance { get; init; }
    public TurretDirection Direction { get; init; }

    public Target((int Y, int X) myPosition, ITank target)
    {
        Tank = target;

        var diffY = myPosition.Y - target.Y;
        var diffX = myPosition.X - target.X;

        Distance = Math.Sqrt(Math.Pow(diffY, 2) + Math.Pow(diffX, 2));

        if (myPosition.Y == target.Y)
        {
            Direction = myPosition.X > target.X ? TurretDirection.East : TurretDirection.West;
        }
        else if (myPosition.X == target.X)
        {
            Direction = myPosition.Y > target.Y ? TurretDirection.South : TurretDirection.North;
        }
        else
        { 
            var angleInDegrees = (Math.Atan2(diffY, diffX) * 180 / Math.PI + 360) % 360;

            Direction = angleInDegrees switch
            {
                >= 315 - 22.5 => TurretDirection.SouthEast,
                >= 270 - 22.5 => TurretDirection.South,
                >= 225 - 22.5 => TurretDirection.SouthWest,
                >= 180 - 22.5 => TurretDirection.West,
                >= 135 - 22.5 => TurretDirection.NorthWest,
                >=  90 - 22.5 => TurretDirection.North,
                >=  45 - 22.5 => TurretDirection.NorthEast,
                _             => TurretDirection.East
            };
        }
    }
}