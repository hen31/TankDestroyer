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

        // note that East and West are flipped :(
        if (myPosition.Y == target.Y)
        {
            Direction = myPosition.X > target.X ? TurretDirection.West : TurretDirection.East;
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
                >= 315 - 22.5 => TurretDirection.SouthWest,
                >= 270 - 22.5 => TurretDirection.South,
                >= 225 - 22.5 => TurretDirection.SouthEast,
                >= 180 - 22.5 => TurretDirection.East,
                >= 135 - 22.5 => TurretDirection.NorthEast,
                >=  90 - 22.5 => TurretDirection.North,
                >=  45 - 22.5 => TurretDirection.NorthWest,
                _             => TurretDirection.West
            };
        }
    }
}