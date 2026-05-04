using TankDestroyer.API;

namespace ANSU.Bot;

[Bot("ANSU TankHunter", "ANSU", "FF6600")]
public class ANSUBot : IPlayerBot
{
    // Coordinate system:
    //   North = Y+1, South = Y-1, East = X-1, West = X+1
    //
    // ENGINE BUG WORKAROUND for GetTile:
    //   PlayerTurnContext.GetTile(y, x) calls World.GetTile(y, x),
    //   but World.GetTile(int x, int y) treats its FIRST arg as X.
    //   Result: ctx.GetTile(A, B) returns Tiles[(B * Width) + A].
    //   To get the correct tile at world (X, Y), call ctx.GetTile(X, Y) — X first.

    public void DoTurn(ITurnContext ctx)
    {
        var me = ctx.Tank;

        var enemies = ctx.GetTanks()
            .Where(t => t.OwnerId != me.OwnerId && !t.Destroyed)
            .ToArray();

        if (enemies.Length == 0) return;

        var target = enemies.OrderBy(e => ManhattanDist(me.X, me.Y, e.X, e.Y)).First();

        // ── Step 1: Determine movement ────────────────────────────────────────
        // Calculate movement first so we can aim from the post-move position.
        // Turn order: rotate(0) → move(30) → fire(1000).
        // Turret is set now but the bullet fires AFTER the tank has moved.
        var moveDir = ChooseMove(ctx, me, target);

        // Predict where we'll be when firing (post-move position)
        int postX = moveDir.HasValue ? NewX(me.X, moveDir.Value) : me.X;
        int postY = moveDir.HasValue ? NewY(me.Y, moveDir.Value) : me.Y;

        // ── Step 2: Rotate turret toward target from post-move position ───────
        var aimDir = GetBestAimDirection(postX, postY, target.X, target.Y);
        ctx.RotateTurret(aimDir);

        // ── Step 3: Submit movement ───────────────────────────────────────────
        if (moveDir.HasValue)
            ctx.MoveTank(moveDir.Value);

        // ── Step 4: Fire if path from post-move position is clear ─────────────
        // A bullet fired from a tree immediately self-destructs, so skip.
        if (GetTileType(ctx, postX, postY) == TileType.Tree) return;

        if (HasClearPath(ctx, postX, postY, target.X, target.Y, aimDir))
            ctx.Fire();
    }

    // ── Movement decision ─────────────────────────────────────────────────────

    private Direction? ChooseMove(ITurnContext ctx, ITank me, ITank target)
    {
        // Priority 1: dodge any incoming bullet
        foreach (var bullet in ctx.GetBullets())
        {
            if (IsBulletThreat(bullet, me.X, me.Y))
            {
                var dodge = GetDodgeDirection(ctx, me, bullet);
                if (dodge.HasValue) return dodge;
            }
        }

        // Priority 2: seek tree cover when health is critical
        if (me.Health <= 25)
        {
            var cover = SeekCover(ctx, me);
            if (cover.HasValue) return cover;
        }

        // Priority 3: align on same row or column as target for a cardinal shot
        return GetAlignmentMove(ctx, me, target);
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

    private static Direction? GetDodgeDirection(ITurnContext ctx, ITank me, IBullet bullet)
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
            if (CanMove(ctx, me, dir)) return dir;

        return null;
    }

    private static Direction? SeekCover(ITurnContext ctx, ITank me)
    {
        foreach (var dir in AllDirections())
        {
            int nx = NewX(me.X, dir), ny = NewY(me.Y, dir);
            if (!InBounds(ctx, nx, ny)) continue;
            if (GetTileType(ctx, nx, ny) == TileType.Tree && CanMove(ctx, me, dir))
                return dir;
        }
        return null;
    }

    private static Direction? GetAlignmentMove(ITurnContext ctx, ITank me, ITank target)
    {
        int dx = me.X - target.X;  // positive → target is East of us
        int dy = target.Y - me.Y;  // positive → target is North of us

        // Already on same column: only close in if still out of range
        if (dx == 0)
        {
            if (Math.Abs(dy) > 6)
            {
                var d = dy > 0 ? Direction.North : Direction.South;
                if (CanMove(ctx, me, d)) return d;
            }
            return null;
        }

        // Already on same row: only close in if still out of range
        if (dy == 0)
        {
            if (Math.Abs(dx) > 6)
            {
                var d = dx > 0 ? Direction.East : Direction.West;
                if (CanMove(ctx, me, d)) return d;
            }
            return null;
        }

        // Not aligned: reduce the smaller offset first to reach alignment faster
        if (Math.Abs(dx) <= Math.Abs(dy))
        {
            var d = dx > 0 ? Direction.East : Direction.West;
            if (CanMove(ctx, me, d)) return d;
        }

        {
            var d = dy > 0 ? Direction.North : Direction.South;
            if (CanMove(ctx, me, d)) return d;
        }

        // Fallback: other axis
        if (Math.Abs(dx) > Math.Abs(dy))
        {
            var d = dx > 0 ? Direction.East : Direction.West;
            if (CanMove(ctx, me, d)) return d;
        }

        return null;
    }

    // ── Firing helpers ────────────────────────────────────────────────────────

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

    // ── Direction arithmetic ──────────────────────────────────────────────────

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

    // ── Tile helpers (engine bug workaround) ──────────────────────────────────
    // ctx.GetTile(A, B) returns Tiles[(B*Width)+A]; to get tile at world (x,y) call ctx.GetTile(x, y).

    private static TileType GetTileType(ITurnContext ctx, int x, int y)
        => ctx.GetTile(x, y).TileType;

    private static bool InBounds(ITurnContext ctx, int x, int y)
        => x >= 0 && x < ctx.GetMapWidth() && y >= 0 && y < ctx.GetMapHeight();

    // ── Movement helpers ──────────────────────────────────────────────────────

    private static bool CanMove(ITurnContext ctx, ITank me, Direction dir)
    {
        int nx = NewX(me.X, dir), ny = NewY(me.Y, dir);

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
