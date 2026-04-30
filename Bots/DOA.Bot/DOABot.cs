using TankDestroyer.API;

namespace DOA.Bot;

[Bot("DOABot", "Lesly", "DC143C")] // crimson
public class DOABot : IPlayerBot
{
    private Random _random = new();

    public void DoTurn(ITurnContext context)
    {
        MoveTank(context);
        AimAndFire(context);
    }

    private void MoveTank(ITurnContext context)
    {
        var currentPosition = (context.Tank.Y, context.Tank.X);

        // 1. Start with all possible (new) positions
        var possiblePositions = new NewPosition[]
        {
            new(currentPosition,  Direction.North),
            new(currentPosition,  Direction.East),
            new(currentPosition,  Direction.South),
            new(currentPosition), // Do Not Move 
            new(currentPosition,  Direction.West),
        } // do not forget to avoid water
        .Where(newPosition => context.GetTile(newPosition.Y, newPosition.X).TileType != TileType.Water);

        // 2. Calculate the possible damage at every possible position        
        foreach (var possiblePosition in possiblePositions)
        {
            var damage = context.GetTile(possiblePosition.Y, possiblePosition.X).TileType switch
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
        var safeDirections = possiblePositions
            .Where(possiblePosition => possiblePosition.Damage == minimalDamage)
            .Select(possiblePosition => possiblePosition.MoveTo)
            .ToArray();

        // 4. Determine the new position and move if needed
        var moveTo = safeDirections[_random.Next(0, safeDirections.Length)];
        if (moveTo.HasValue)
        {
            context.MoveTank(moveTo.Value);
        }
    }

    private void AimAndFire(ITurnContext context)
    {
        var currentPosition = (context.Tank.Y, context.Tank.X);

        if (context.GetTile(currentPosition.Y, currentPosition.X).TileType == TileType.Tree)
        { // 0. We can not fire when we are hidden from a tree
            return;
        }

        // 1. Prepare all possible targets
        var possibleTargets = new List<Target>();
        foreach (var tank in context.GetTanks())
        {
            if (tank.Equals(context.Tank)) // check that we do not aim on ourself ;-)
            {
                continue;
            }

            possibleTargets.Add(new Target(currentPosition, target: tank));
        }

        // 2. Find our preferred target
        var minimalDistance = possibleTargets.Min(possibleTarget => possibleTarget.Distance);
        var aimDirection = possibleTargets
            .First(possibleTarget => possibleTarget.Distance == minimalDistance)
            .Direction;

        // 3. Aim and fire
        context.RotateTurret(aimDirection);
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

        return direction == Opposite(bullet.Direction);
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