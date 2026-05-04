using TankDestroyer.API;

namespace TankDestroyer.Engine;

public class MoveTankAction : TankAction
{
    private readonly Direction _moveDirection;

    public MoveTankAction(int ownerId, Direction moveDirection) : base(ownerId)
    {
        _moveDirection = moveDirection;
        Priority = 30;
    }

    internal override bool Execute(Game game)
    {
        var tank = game.Tanks.FirstOrDefault(c => c.OwnerId == OwnerId);
        if (tank == null)
        {
            return false;
        }

        switch (_moveDirection)
        {
            case Direction.North:
                if (tank.Y == 0)
                {
                    return false;
                }

                if (!IsPassable(tank.X, tank.Y - 1, game))
                {
                    return false;
                }

                tank.Y -= 1;
                return true;
            case Direction.South:
                if (tank.Y == game.World.Height - 1)
                {
                    return false;
                }

                if (!IsPassable(tank.X, tank.Y + 1, game))
                {
                    return false;
                }


                tank.Y += 1;
                return true;
            case Direction.East:
                if (tank.X == game.World.Width - 1)
                {
                    return false;
                }

                if (!IsPassable(tank.X + 1, tank.Y, game))
                {
                    return false;
                }

                tank.X += 1;
                return true;
            case Direction.West:
                if (tank.X == 0)
                {
                    return false;
                }

                if (!IsPassable(tank.X - 1, tank.Y, game))
                {
                    return false;
                }

                tank.X -= 1;
                return true;
            default:
                return false;
        }
    }

    private bool IsPassable(int tankX, int tankY, Game game)
    {
        if (game.Tanks.Any(c => c.X == tankX && c.Y == tankY)
            || game.World.GetTile(tankX, tankY).TileType == TileType.Water)
        {
            return false;
        }

        return true;
    }
}