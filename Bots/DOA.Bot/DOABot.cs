using TankDestroyer.API;

namespace DOA.Bot;

[Bot("DOABot", "Lesly", "DC143C")] // crimson
public class DOABot : IPlayerBot
{
    public void DoTurn(ITurnContext context)
    {
        MoveTank(context);
        AimAndFire(context);
    }

    private void MoveTank(ITurnContext context)
    {
        var currentPosition = (context.Tank.Y, context.Tank.X);

        // 1. Start with all possible (new) positions
        var possiblePositions = new List<NewPosition>()
        {
            new(currentPosition), // Do Not Move 
        };
        foreach (var direction in Enum.GetValues<Direction>())
        {
            if (NewPosition.PositionExists(currentPosition, direction, context))
            {
                possiblePositions.Add(new NewPosition(currentPosition,  direction));
            }
        }
        possiblePositions = [.. possiblePositions // do not forget to avoid water
            .Where(newPosition => context.GetTile(newPosition.X, newPosition.Y).TileType != TileType.Water)];

        // 2. Calculate the possible damage at every possible position        
        foreach (var possiblePosition in possiblePositions)
        {
            var damage = context.GetTile(possiblePosition.X, possiblePosition.Y).TileType switch
            {
                TileType.Tree => 25,
                TileType.Building => 50,
                _ => 75,
            };

            foreach (var bullet in context.GetBullets())
            {
                if (AboutToGetHit(bullet, (possiblePosition.Y, possiblePosition.X)))
                {
                    possiblePosition.Damage += damage;
                }
            }
        }

        // 3. Get the positions where we will receive minimal damage
        var minimalDamage = possiblePositions.Min(possiblePosition => possiblePosition.Damage);
        var safestPositions = possiblePositions
            .Where(possiblePosition => possiblePosition.Damage == minimalDamage)
            .ToArray();

        // 4. Determine the new position and move if needed
        var newPosition = FindBestPosition(safestPositions, context);
        if (newPosition.MoveTo.HasValue)
        {
            context.MoveTank(newPosition.MoveTo.Value);
        }
    }

    private TurretDirection Aim(ITurnContext context)
    {
        var currentPosition = (context.Tank.Y, context.Tank.X);

        // 1. Prepare all possible targets
        var possibleTargets = new List<Target>();
        foreach (var tank in context.GetTanks())
        {
            if (tank.Equals(context.Tank)) // check that we do not aim on ourself ;-)
            {
                continue;
            }
            if (tank.Destroyed)
            {
                continue;
            }

            possibleTargets.Add(new Target(currentPosition, target: tank));
        }

        // 2. Find our preferred target
        var minimalDistance = possibleTargets.Min(possibleTarget => possibleTarget.Distance);
        return possibleTargets
            .First(possibleTarget => possibleTarget.Distance == minimalDistance)
            .Direction;
    }

    private void AimAndFire(ITurnContext context)
    {
        context.RotateTurret(Aim(context));
        context.Fire();
    }

    private bool AboutToGetHit(IBullet bullet, (int Y, int X) position)
    {
        var diffY = position.Y - bullet.Y;
        var diffX = position.X - bullet.X;

        if (Math.Sqrt(Math.Pow(diffY, 2) + Math.Pow(diffX, 2)) > 6) // distance
        {
            return false;
        }
        
        var angleInDegrees = (Math.Atan2(diffY, diffX) * 180 / Math.PI + 360) % 360;

        var direction = angleInDegrees switch
        { // note that East and West are flipped :(
            >= 315 - 22.5 => TurretDirection.SouthWest,
            >= 270 - 22.5 => TurretDirection.South,
            >= 225 - 22.5 => TurretDirection.SouthEast,
            >= 180 - 22.5 => TurretDirection.East,
            >= 135 - 22.5 => TurretDirection.NorthEast,
            >=  90 - 22.5 => TurretDirection.North,
            >=  45 - 22.5 => TurretDirection.NorthWest,
            _             => TurretDirection.West
        };

        return direction == Opposite(bullet.Direction);
    }

    private NewPosition FindBestPosition(NewPosition[] possiblePositions, ITurnContext context)
    {
        NewPosition newPosition = possiblePositions[0];
        if (possiblePositions.Length == 1)
        {
            return newPosition;
        }

        Direction[] aimDirections = Aim(context) switch
        {
            TurretDirection.North     => [Direction.North],
            TurretDirection.NorthEast => [Direction.North, Direction.East],
            TurretDirection.East      => [Direction.East],
            TurretDirection.SouthEast => [Direction.South, Direction.East],
            TurretDirection.South     => [Direction.South],
            TurretDirection.SouthWest => [Direction.South, Direction.West],
            TurretDirection.West      => [Direction.West],
            TurretDirection.NorthWest => [Direction.North, Direction.West],

            _ => throw new NotSupportedException()
        };

        double highestScore = double.MinValue;
        foreach (var possiblePosition in possiblePositions)
        {
            var score = context.GetTile(possiblePosition.X, possiblePosition.Y).TileType switch
            {
                TileType.Tree => 25,
                TileType.Building => 50,
                _ => 0,
            }
            +
            possiblePosition.MoveTo switch
            {
                null => 0.25,
                _ when aimDirections.Contains(possiblePosition.MoveTo.Value) => (aimDirections.Length == 1) ? 1.25 : 0.75,
                _ => -0.25,
            } * 100;

            if (score > highestScore)
            {
                highestScore = score;
                newPosition = possiblePosition;
            }
        }

        return newPosition;
    }

    private static TurretDirection Opposite(TurretDirection direction) => direction switch
    {
        TurretDirection.North     => TurretDirection.South,
        TurretDirection.NorthEast => TurretDirection.SouthWest,
        TurretDirection.East      => TurretDirection.West,
        TurretDirection.SouthEast => TurretDirection.NorthWest,
        TurretDirection.South     => TurretDirection.North,
        TurretDirection.SouthWest => TurretDirection.NorthEast,
        TurretDirection.West      => TurretDirection.East,
        TurretDirection.NorthWest => TurretDirection.SouthEast,

        _ => throw new NotSupportedException()
    };
}