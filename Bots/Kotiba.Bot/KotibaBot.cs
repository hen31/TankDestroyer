using TankDestroyer.API;

namespace Kotiba.Bot;

[Bot("Kotiba bot", "By Kotiba", "0ad859")]
public class KotibaBot : IPlayerBot
{
    public void DoTurn(ITurnContext turnContext)
    {
        var myTank = turnContext.Tank;
        var enemies = turnContext.GetTanks().Where(t => !t.Destroyed && t.OwnerId != myTank.OwnerId).ToArray();
        if (enemies.Length == 0) return;

        var closestEnemy = enemies.OrderBy(e => Math.Abs(e.X - myTank.X) + Math.Abs(e.Y - myTank.Y)).First();

        int deltaX = closestEnemy.X - myTank.X;
        int deltaY = closestEnemy.Y - myTank.Y;

        TurretDirection targetTurretDir = 0;
        if (Math.Abs(deltaY) > Math.Abs(deltaX))
        {
            targetTurretDir = deltaY > 0 ? TurretDirection.North : TurretDirection.South;
        }
        else
        {
            targetTurretDir = deltaX > 0 ? TurretDirection.West : TurretDirection.East;
        }

        var currentTile = turnContext.GetTile(myTank.Y, myTank.X);
        if (currentTile != null && currentTile.TileType == TileType.Tree)
        {
            var directions = new[] { Direction.North, Direction.East, Direction.South, Direction.West };
            foreach (var dir in directions.OrderBy(_ => Random.Shared.Next()))
            {
                if (CanMove(turnContext, dir) && turnContext.GetTile(GetNewY(myTank.Y, dir), GetNewX(myTank.X, dir)).TileType == TileType.Grass)
                {
                    turnContext.MoveTank(dir);
                    return;
                }
            }
            foreach (var dir in directions.OrderBy(_ => Random.Shared.Next()))
            {
                if (CanMove(turnContext, dir))
                {
                    turnContext.MoveTank(dir);
                    return;
                }
            }
            return;
        }

        if (myTank.TurretDirection != targetTurretDir)
        {
            turnContext.RotateTurret(targetTurretDir);
            return;
        }

        if (IsPathClear(turnContext, myTank, closestEnemy, targetTurretDir))
        {
            turnContext.Fire();
            return;
        }

        var possibleMoves = new[] { Direction.North, Direction.East, Direction.South, Direction.West }
            .Where(d => CanMove(turnContext, d))
            .Select(d => new { Dir = d, NewX = GetNewX(myTank.X, d), NewY = GetNewY(myTank.Y, d) })
            .Select(m => new { m.Dir, Distance = Math.Abs(m.NewX - closestEnemy.X) + Math.Abs(m.NewY - closestEnemy.Y) })
            .OrderBy(m => m.Distance)
            .ThenBy(_ => Random.Shared.Next())
            .FirstOrDefault();

        if (possibleMoves != null)
        {
            turnContext.MoveTank(possibleMoves.Dir);
            return;
        }
    }

    private bool CanMove(ITurnContext context, Direction dir)
    {
        int newX = GetNewX(context.Tank.X, dir);
        int newY = GetNewY(context.Tank.Y, dir);
        if (newX < 0 || newX >= context.GetMapWidth() || newY < 0 || newY >= context.GetMapHeight()) return false;
        var tile = context.GetTile(newY, newX);
        return tile.TileType != TileType.Water;
    }

    private int GetNewX(int x, Direction dir)
    {
        return dir switch
        {
            Direction.East => x - 1,
            Direction.West => x + 1,
            _ => x
        };
    }

    private int GetNewY(int y, Direction dir)
    {
        return dir switch
        {
            Direction.North => y + 1,
            Direction.South => y - 1,
            _ => y
        };
    }

    private Direction GetMoveDirection(int deltaX, int deltaY)
    {
        if (Math.Abs(deltaY) > Math.Abs(deltaX))
        {
            return deltaY > 0 ? Direction.North : Direction.South;
        }
        else
        {
            return deltaX > 0 ? Direction.West : Direction.East;
        }
    }

    private bool IsPathClear(ITurnContext context, ITank myTank, ITank enemy, TurretDirection dir)
    {
        int distance;
        if (dir == TurretDirection.North || dir == TurretDirection.South)
        {
            distance = Math.Abs(enemy.Y - myTank.Y);
        }
        else if (dir == TurretDirection.East || dir == TurretDirection.West)
        {
            distance = Math.Abs(enemy.X - myTank.X);
        }
        else
        {
            return false;
        }
        if (distance > 6 || distance == 0) return false;

        if (dir == TurretDirection.North)
        {
            if (enemy.X != myTank.X || enemy.Y <= myTank.Y) return false;
            for (int y = myTank.Y + 1; y < enemy.Y; y++)
            {
                if (y >= context.GetMapHeight()) return false;
                var tile = context.GetTile(y, myTank.X);
                if (tile == null || tile.TileType == TileType.Tree || tile.TileType == TileType.Building) return false;
            }
            return true;
        }
        else if (dir == TurretDirection.South)
        {
            if (enemy.X != myTank.X || enemy.Y >= myTank.Y) return false;
            for (int y = myTank.Y - 1; y > enemy.Y; y--)
            {
                if (y < 0) return false;
                var tile = context.GetTile(y, myTank.X);
                if (tile == null || tile.TileType == TileType.Tree || tile.TileType == TileType.Building) return false;
            }
            return true;
        }
        else if (dir == TurretDirection.East)
        {
            if (enemy.Y != myTank.Y || enemy.X >= myTank.X) return false;
            for (int x = myTank.X - 1; x > enemy.X; x--)
            {
                if (x < 0) return false;
                var tile = context.GetTile(myTank.Y, x);
                if (tile == null || tile.TileType == TileType.Tree || tile.TileType == TileType.Building) return false;
            }
            return true;
        }
        else if (dir == TurretDirection.West)
        {
            if (enemy.Y != myTank.Y || enemy.X <= myTank.X) return false;
            for (int x = myTank.X + 1; x < enemy.X; x++)
            {
                if (x >= context.GetMapWidth()) return false;
                var tile = context.GetTile(myTank.Y, x);
                if (tile == null || tile.TileType == TileType.Tree || tile.TileType == TileType.Building) return false;
            }
            return true;
        }
        return false;
    }
}