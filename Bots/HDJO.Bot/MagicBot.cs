using TankDestroyer.API;

namespace Magic.Bot;

[Bot("The coward", "Olivier Bierbooms", "5865F2")]
public class MagicBot : IPlayerBot
{
    private Random _random = new();

    public void DoTurn(ITurnContext turnContext)
    {
        var myTank = turnContext.Tank;
        var allTanks = turnContext.GetTanks().Where(t => !t.Destroyed).ToList();
        var opponents = allTanks.Where(t => t.OwnerId != myTank.OwnerId).ToList();

        if (opponents.Count > 1)
        {
            DoSurvivalTurn(turnContext, myTank, opponents);
        }
        else if (opponents.Count == 1)
        {
            DoAggressiveTurn(turnContext, myTank, opponents[0]);
        }
        else 
        {
            // Just move randomly and shoot if no opponents
            var enumDirectionValues = Enum.GetValues<Direction>();
            turnContext.MoveTank(enumDirectionValues[_random.Next(0, enumDirectionValues.Length)]);
            var enumValues = Enum.GetValues<TurretDirection>();
            turnContext.RotateTurret(enumValues[_random.Next(0, enumValues.Length)]);
        }
    }

    private void DoSurvivalTurn(ITurnContext turnContext, ITank myTank, List<ITank> opponents)
    {
        var mapWidth = turnContext.GetMapWidth();
        var mapHeight = turnContext.GetMapHeight();

        ITile bestTile = null;
        double maxDistToClosestOpponent = -1;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                var tile = turnContext.GetTile(y, x);
                if (tile.TileType == TileType.Tree || tile.TileType == TileType.Building)
                {
                    double distToClosestOpponent = opponents.Min(o => GetDistance(tile.X, tile.Y, o.X, o.Y));
                    if (distToClosestOpponent > maxDistToClosestOpponent)
                    {
                        maxDistToClosestOpponent = distToClosestOpponent;
                        bestTile = tile;
                    }
                }
            }
        }

        int nextX = myTank.X;
        int nextY = myTank.Y;

        if (bestTile != null)
        {
            MoveTowards(turnContext, myTank.X, myTank.Y, bestTile.X, bestTile.Y, out nextX, out nextY);
        }

        var nextTile = turnContext.GetTile(nextY, nextX);
        if (nextTile != null && nextTile.TileType != TileType.Tree)
        {
            var target = opponents.OrderBy(o => GetDistance(nextX, nextY, o.X, o.Y)).FirstOrDefault();
            if (target != null)
            {
                AimAndFireAt(turnContext, nextX, nextY, target);
            }
        }
    }

    private void DoAggressiveTurn(ITurnContext turnContext, ITank myTank, ITank target)
    {
        MoveTowards(turnContext, myTank.X, myTank.Y, target.X, target.Y, out int nextX, out int nextY);
        AimAndFireAt(turnContext, nextX, nextY, target);
    }

    private double GetDistance(int x1, int y1, int x2, int y2)
    {
        return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
    }

    private void MoveTowards(ITurnContext turnContext, int currentX, int currentY, int targetX, int targetY, out int nextX, out int nextY)
    {
        nextX = currentX;
        nextY = currentY;

        if (currentX == targetX && currentY == targetY) return;

        int deltaX = targetX - currentX;
        int deltaY = targetY - currentY;

        if (Math.Abs(deltaX) > Math.Abs(deltaY))
        {
            if (deltaX > 0) { turnContext.MoveTank(Direction.West); nextX++; }
            else { turnContext.MoveTank(Direction.East); nextX--; }
        }
        else
        {
            if (deltaY > 0) { turnContext.MoveTank(Direction.North); nextY++; }
            else { turnContext.MoveTank(Direction.South); nextY--; }
        }
    }

    private void AimAndFireAt(ITurnContext turnContext, int myX, int myY, ITank target)
    {
        int deltaX = target.X - myX;
        int deltaY = target.Y - myY;

        TurretDirection dir = TurretDirection.North; // default

        if (deltaX == 0 && deltaY > 0) dir = TurretDirection.North;
        else if (deltaX == 0 && deltaY < 0) dir = TurretDirection.South;
        else if (deltaY == 0 && deltaX > 0) dir = TurretDirection.West; 
        else if (deltaY == 0 && deltaX < 0) dir = TurretDirection.East; 
        else if (deltaX > 0 && deltaY > 0) dir = TurretDirection.NorthWest; 
        else if (deltaX > 0 && deltaY < 0) dir = TurretDirection.SouthWest;
        else if (deltaX < 0 && deltaY > 0) dir = TurretDirection.NorthEast;
        else if (deltaX < 0 && deltaY < 0) dir = TurretDirection.SouthEast;

        turnContext.RotateTurret(dir);
        turnContext.Fire();
    }
}