using TankDestroyer.API;

namespace Example.Bot;

[Bot("Aggressive", "Kevin & Deon", "F527A6")]
public class DeonKevinBot : IPlayerBot
{
    public void DoTurn(ITurnContext turnContext)
    {
        var tanks = turnContext.GetTanks();
        var ourTank = turnContext.Tank;

        var target = GetClosestTank(tanks, ourTank);

        var movementDirection = GetMoveDirection(ourTank, target, turnContext);

        var turretRotation = GetTurretRotation(ourTank, movementDirection, target);

        //var enumValues = Enum.GetValues<TurretDirection>();
        //var enumDirectionValues = Enum.GetValues<Direction>();

        if (movementDirection is not null)
        {
            turnContext.MoveTank(movementDirection.Value); //enumDirectionValues[_random.Next(0, enumDirectionValues.Length)]
        }
        turnContext.RotateTurret(turretRotation.Value);

        turnContext.Fire();
    }

    private TurretDirection? GetTurretRotation(ITank ourTank, Direction? movementDirection, ITank target)
    {
        var nextPos = GetNextPosition(ourTank, movementDirection ?? Direction.North);

        var xDiff = nextPos.X - target.X;
        var yDiff = nextPos.Y - target.Y;

        if (Math.Abs(xDiff) > Math.Abs(yDiff))
        {
            return xDiff < 0 ? TurretDirection.West : TurretDirection.East;
        }
        else
        {
            return yDiff < 0 ? TurretDirection.North : TurretDirection.South;
        }

    }

    public ITank? GetClosestTank(ITank[] tanks, ITank ourTank)
    {
        ITank? closestTank = null;
        var closestDistance = int.MaxValue;

        foreach (var tank in tanks)
        {
            if (tank.OwnerId == ourTank.OwnerId) continue;
            if (tank.Destroyed) continue;

            var dx = Math.Abs(tank.X - ourTank.X);
            var dy = Math.Abs(tank.Y - ourTank.Y);
            var distance = dx * dx + dy * dy;
            if (closestTank == null || distance < closestDistance)
            {
                closestTank = tank;
                closestDistance = distance;
            }
        }

        return closestTank;
    }

    private Direction? GetMoveDirection(ITank our, ITank target, ITurnContext turnContext)
    {
        var dx = target.X - our.X;
        var dy = target.Y - our.Y;

        if (dx == 0 && dy == 0) return null;


        var nextDirection = Direction.North;
        var secondBestDirection = Direction.South;

        if (Math.Abs(dx) > Math.Abs(dy))
        {
            nextDirection = dx > 0 ? Direction.West : Direction.East;
            secondBestDirection = dy > 0 ? Direction.North : Direction.South;
        }
        else
        {
            nextDirection = dy > 0 ? Direction.North : Direction.South;
            secondBestDirection = dx > 0 ? Direction.West : Direction.East;
        }

        List<Direction> directionsToCheck = new() { nextDirection, secondBestDirection };

        if (nextDirection == Direction.North)
        {
            directionsToCheck.Add(Direction.South);
        }
        else
        {
            directionsToCheck.Add(Direction.North);
        }

        if (secondBestDirection == Direction.East)
        {
            directionsToCheck.Add(Direction.West);
        }
        else
        {
            directionsToCheck.Add(Direction.East);
        }

        foreach (var direction in directionsToCheck)
        {
            if (IsLegalTile(our, direction, turnContext))
            {
                return direction;
            }

        }

        return nextDirection;
    }

    private bool IsLegalTile(ITank ourTank, Direction direction, ITurnContext turnContext)
    {
        (int X, int Y) nextPos = GetNextPosition(ourTank, direction);

        Console.WriteLine($"Checking tile at {nextPos.X}, {nextPos.Y} for direction {direction}");
        var tile = turnContext.GetTile(nextPos.X, nextPos.Y);

        if (tile == null) { return false; }
        return tile.TileType == TileType.Water;
    }

    private static (int X, int Y) GetNextPosition(ITank ourTank, Direction direction)
    {
        var nextPos = (ourTank.X, ourTank.Y);
        switch (direction)
        {
            case Direction.North:
                nextPos.X += 1;
                break;
            case Direction.West:
                nextPos.Y += 1;
                break;
            case Direction.South:
                nextPos.X -= 1;
                break;
            case Direction.East:
                nextPos.Y -= 1;
                break;
        }

        return nextPos;
    }
}