using TankDestroyer.API;

namespace LISM.Bot;

[Bot("Bee Bot", "Lien", "E9AB17")]
public class BeeBot : IPlayerBot
{
    private const int MaxMovement = 6;
    private Random _random = new();
    private int Height;
    private int Width;


    public void DoTurn(ITurnContext turnContext)
    {
        Height = turnContext.GetMapHeight();
        Width = turnContext.GetMapWidth();

        var myPosition = new Position(turnContext.Tank.X, turnContext.Tank.Y);

        if (turnContext.GetBullets().Any(bullet => !IsPositionSafe(myPosition, bullet)))
        {
            turnContext.MoveTank(NextBestMove(turnContext.Tank, turnContext.GetBullets()));
        }

        var enemy = turnContext.GetTanks()
            .Where(tank => tank.OwnerId != turnContext.Tank.OwnerId)
            .OrderBy(enemy => CalculateClosenessEnemy(myPosition, enemy))
            .FirstOrDefault();
        if (enemy is not null)
        {
            var enemyDirection = RelativePositionBasedOnSecond(new Position(enemy.X, enemy.Y), myPosition, false);
            if (enemyDirection is not null)
            {
                turnContext.RotateTurret((TurretDirection)enemyDirection);
            }
        }

        turnContext.Fire();
    }

    private int CalculateClosenessEnemy(Position position, ITank enemy)
        => Math.Abs(position.X - enemy.X) + Math.Abs(position.Y - enemy.Y);

    private Direction NextBestMove(ITank tank, IBullet[] bullets)
    {
        if (bullets.All(bullet => IsPositionSafe(new Position(tank.X + 1, tank.Y), bullet)))
        {
            return Direction.East;
        }

        if (bullets.All(bullet => IsPositionSafe(new Position(tank.X - 1, tank.Y), bullet)))
        {
            return Direction.West;
        }

        if (bullets.All(bullet => IsPositionSafe(new Position(tank.X, tank.Y + 1), bullet)))
        {
            return Direction.North;
        }

        if (bullets.All(bullet => IsPositionSafe(new Position(tank.X, tank.Y - 1), bullet)))
        {
            return Direction.South;
        }

        return Enum.GetValues<Direction>()[_random.Next(0, Enum.GetValues<Direction>().Length)];
    }


    private TurretDirection? RelativePositionBasedOnSecond(Position position1, Position position2, bool useMaxMovement)
    {
        var maxMovement = useMaxMovement ? MaxMovement : 0;

        if (position1.X != position2.X || position1.Y < position2.Y - maxMovement)
        {
            return TurretDirection.North;
        }

        if (position1.X != position2.X || position1.Y > position2.Y + maxMovement)
        {
            return TurretDirection.South;
        }

        if (position1.Y != position2.Y || position1.X > position2.X + maxMovement)
        {
            return TurretDirection.East;
        }

        if (position1.Y != position2.Y || position1.X < position2.X - maxMovement)
        {
            return TurretDirection.West;
        }

        if (IsDiagonalPositionSafe(position1, position2, true, true, useMaxMovement))
        {
            return TurretDirection.NorthEast;
        }

        if (IsDiagonalPositionSafe(position1, position2, true, false, useMaxMovement))
        {
            return TurretDirection.SouthEast;
        }

        if (IsDiagonalPositionSafe(position1, position2, false, false, useMaxMovement))
        {
            return TurretDirection.SouthWest;
        }

        if (IsDiagonalPositionSafe(position1, position2, false, true, useMaxMovement))
        {
            return TurretDirection.NorthWest;
        }

        return null;
    }

    private bool IsPositionSafe(Position position, IBullet bullet)
    {
        var direction = RelativePositionBasedOnSecond(position, new Position(bullet.X, bullet.Y), true);
        return direction is null
               || direction != bullet.Direction;

        // return bullet.Direction switch
        // {
        //     TurretDirection.North => position.X != bullet.X || position.Y < bullet.Y - MaxMovement,
        //     TurretDirection.South => position.X != bullet.X || position.Y > bullet.Y + MaxMovement,
        //     TurretDirection.East => position.Y != bullet.Y || position.X > bullet.X + MaxMovement,
        //     TurretDirection.West => position.Y != bullet.Y || position.X < bullet.X - MaxMovement,
        //     TurretDirection.NorthEast => IsDiagonalPositionSafe(new Position(position.X, position.Y),
        //         new Position(bullet.X, bullet.Y), true, true),
        //     TurretDirection.SouthEast => IsDiagonalPositionSafe(new Position(position.X, position.Y),
        //         new Position(bullet.X, bullet.Y), true, false),
        //     TurretDirection.SouthWest => IsDiagonalPositionSafe(new Position(position.X, position.Y),
        //         new Position(bullet.X, bullet.Y), false, false),
        //     TurretDirection.NorthWest => IsDiagonalPositionSafe(new Position(position.X, position.Y),
        //         new Position(bullet.X, bullet.Y), false, true),
        //     _ => false
        // };
    }

    private bool IsDiagonalPositionSafe(Position position1, Position position2, bool upX, bool upY, bool useMaxMovement)
    {
        var loopChangeX = upX ? 1 : -1;
        var loopChangeY = upY ? 1 : -1;
        var maxi = useMaxMovement ? MaxMovement : Math.Max(Height, Width);

        for (var (x, y, i) = (position2.X, position2.Y, 0); i < maxi; i++, x += loopChangeX, y += loopChangeY)
        {
            if (x == position1.X && y == position1.Y)
            {
                return false;
            }
        }

        return true;
    }
}