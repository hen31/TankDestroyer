using TankDestroyer.API;

namespace ANSU.Bot;

[Bot("ANSU TankHunter", "ANSU", "FF6600")]
public class ANSUBot : IPlayerBot
{
    public void DoTurn(ITurnContext ctx)
    {
        var myTank = ctx.Tank;

        var enemies = ctx.GetTanks()
            .Where(tank => tank.OwnerId != myTank.OwnerId && !tank.Destroyed)
            .ToArray();

        if (enemies.Length == 0) return;

        var target = enemies.OrderBy(e => ManhattanDist(myTank.X, myTank.Y, e.X, e.Y)).First();

        var moveDirection = ChooseMove(ctx, myTank, target);

        int postX = moveDirection.HasValue ? NewX(myTank.X, moveDirection.Value) : myTank.X;
        int postY = moveDirection.HasValue ? NewY(myTank.Y, moveDirection.Value) : myTank.Y;

        var aimDirection = GetBestAimDirection(postX, postY, target.X, target.Y);
        ctx.RotateTurret(aimDirection);

        if (moveDirection.HasValue)
            ctx.MoveTank(moveDirection.Value);

        if (GetTileType(ctx, postX, postY) == TileType.Tree) return;

        if (HasClearPath(ctx, postX, postY, target.X, target.Y, aimDirection))
            ctx.Fire();
    }

    private Direction? ChooseMove(ITurnContext ctx, ITank myTank, ITank target)
    {
        foreach (var bullet in ctx.GetBullets())
        {
            if (IsBulletThreat(bullet, myTank.X, myTank.Y))
            {
                var dodge = GetDodgeDirection(ctx, myTank, bullet);
                if (dodge.HasValue) return dodge;
            }
        }

        if (myTank.Health <= 25)
        {
            var cover = SeekCover(ctx, myTank);
            if (cover.HasValue) return cover;
        }

        return GetAlignmentMove(ctx, myTank, target);
    }

    private static bool IsBulletThreat(IBullet bullet, int myX, int myY)
    {
        int sx = 0, sy = 0;
        GetStep(bullet.Direction, ref sx, ref sy);

        for (int i = 1; i <= 6; i++)
        {
            if (bullet.X + sx * i == myX && bullet.Y + sy * i == myY) return true;
        }
        return false;
    }

    private static Direction? GetDodgeDirection(ITurnContext ctx, ITank myTank, IBullet bullet)
    {
        bool ns = bullet.Direction.HasFlag(TurretDirection.North) ||
                  bullet.Direction.HasFlag(TurretDirection.South);
        bool ew = bullet.Direction.HasFlag(TurretDirection.East) ||
                  bullet.Direction.HasFlag(TurretDirection.West);

        Direction[] candidates = (ns && !ew)
            ? new[] { Direction.East, Direction.West }
            : (ew && !ns)
                ? new[] { Direction.North, Direction.South }
                : new[] { Direction.North, Direction.South, Direction.East, Direction.West };

        foreach (var dir in candidates)
            if (CanMove(ctx, myTank, dir)) return dir;

        return null;
    }

    private static Direction? SeekCover(ITurnContext ctx, ITank myTank)
    {
        foreach (var dir in AllDirections())
        {
            int nx = NewX(myTank.X, dir), ny = NewY(myTank.Y, dir);
            if (!InBounds(ctx, nx, ny)) continue;
            if (GetTileType(ctx, nx, ny) == TileType.Tree && CanMove(ctx, myTank, dir))
                return dir;
        }
        return null;
    }

    private static Direction? GetAlignmentMove(ITurnContext ctx, ITank myTank, ITank target)
    {
        int dx = myTank.X - target.X; 
        int dy = target.Y - myTank.Y;

        if (dx == 0)
        {
            if (Math.Abs(dy) > 6)
            {
                var d = dy > 0 ? Direction.North : Direction.South;
                if (CanMove(ctx, myTank, d)) return d;
            }
            return null;
        }

        if (dy == 0)
        {
            if (Math.Abs(dx) > 6)
            {
                var d = dx > 0 ? Direction.East : Direction.West;
                if (CanMove(ctx, myTank, d)) return d;
            }
            return null;
        }

        if (Math.Abs(dx) <= Math.Abs(dy))
        {
            var d = dx > 0 ? Direction.East : Direction.West;
            if (CanMove(ctx, myTank, d)) return d;
        }

        {
            var d = dy > 0 ? Direction.North : Direction.South;
            if (CanMove(ctx, myTank, d)) return d;
        }

        if (Math.Abs(dx) > Math.Abs(dy))
        {
            var d = dx > 0 ? Direction.East : Direction.West;
            if (CanMove(ctx, myTank, d)) return d;
        }

        return null;
    }

    private static bool HasClearPath(ITurnContext ctx, int fromX, int fromY,
                                     int toX, int toY, TurretDirection dir)
    {
        int sx = 0, sy = 0;
        GetStep(dir, ref sx, ref sy);

        int x = fromX + sx, y = fromY + sy;
        for (int i = 0; i < 6; i++)
        {
            if (!InBounds(ctx, x, y)) return false;
            if (x == toX && y == toY) return true;

            var tt = GetTileType(ctx, x, y);
            if (tt == TileType.Tree || tt == TileType.Building) return false;

            x += sx;
            y += sy;
        }
        return false;
    }

    private static TurretDirection GetBestAimDirection(int myX, int myY, int tX, int tY)
    {
        var exact = GetExactDirection(myX, myY, tX, tY);
        if (exact.HasValue) return exact.Value;

        int dx = myX - tX;
        int dy = tY - myY;

        double angle = Math.Atan2(dy, dx) * 180.0 / Math.PI;
        if (angle < 0) angle += 360;

        if (angle >= 67.5 && angle < 112.5) return TurretDirection.North;
        if (angle >= 22.5 && angle < 67.5) return TurretDirection.NorthEast;
        if (angle >= 337.5 || angle < 22.5) return TurretDirection.East;
        if (angle >= 292.5 && angle < 337.5) return TurretDirection.SouthEast;
        if (angle >= 247.5 && angle < 292.5) return TurretDirection.South;
        if (angle >= 202.5 && angle < 247.5) return TurretDirection.SouthWest;
        if (angle >= 157.5 && angle < 202.5) return TurretDirection.West;
        return TurretDirection.NorthWest;
    }

    private static TurretDirection? GetExactDirection(int myX, int myY, int tX, int tY)
    {
        int dx = myX - tX;
        int dy = tY - myY;

        if (dx == 0 && dy > 0) return TurretDirection.North;
        if (dx == 0 && dy < 0) return TurretDirection.South;
        if (dy == 0 && dx > 0) return TurretDirection.East;
        if (dy == 0 && dx < 0) return TurretDirection.West;
        if (dx > 0 && dy > 0 && dx == dy) return TurretDirection.NorthEast;
        if (dx < 0 && dy > 0 && -dx == dy) return TurretDirection.NorthWest;
        if (dx > 0 && dy < 0 && dx == -dy) return TurretDirection.SouthEast;
        if (dx < 0 && dy < 0 && dx == dy) return TurretDirection.SouthWest;
        return null;
    }

    private static void GetStep(TurretDirection dir, ref int sx, ref int sy)
    {
        if (dir.HasFlag(TurretDirection.North)) sy += 1;
        if (dir.HasFlag(TurretDirection.South)) sy -= 1;
        if (dir.HasFlag(TurretDirection.East)) sx -= 1;
        if (dir.HasFlag(TurretDirection.West)) sx += 1;
    }

    private static TileType GetTileType(ITurnContext ctx, int x, int y)
        => ctx.GetTile(x, y).TileType;

    private static bool InBounds(ITurnContext ctx, int x, int y)
        => x >= 0 && x < ctx.GetMapWidth() && y >= 0 && y < ctx.GetMapHeight();

    private static bool CanMove(ITurnContext ctx, ITank myTank, Direction dir)
    {
        int nx = NewX(myTank.X, dir), ny = NewY(myTank.Y, dir);

        if (!InBounds(ctx, nx, ny)) return false;
        if (GetTileType(ctx, nx, ny) == TileType.Water) return false;
        return ctx.GetTanks().All(t => t.X != nx || t.Y != ny);
    }

    private static int NewX(int x, Direction dir) => dir switch
    {
        Direction.East => x - 1,
        Direction.West => x + 1,
        _ => x
    };

    private static int NewY(int y, Direction dir) => dir switch
    {
        Direction.North => y + 1,
        Direction.South => y - 1,
        _ => y
    };

    private static IEnumerable<Direction> AllDirections() =>
        new[] { Direction.North, Direction.South, Direction.East, Direction.West };

    private static int ManhattanDist(int x1, int y1, int x2, int y2)
        => Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
}
