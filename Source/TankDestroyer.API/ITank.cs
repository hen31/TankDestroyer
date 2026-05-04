namespace TankDestroyer.API;

public interface ITank
{
    int X { get; }
    int Y { get; }
    int Health { get; }
    TurretDirection TurretDirection { get; }
    bool Destroyed { get; }
    bool Fired { get; }
    int OwnerId { get;  }
}

[Flags]
public enum Direction
{
    /// <summary>
    /// Y - 1
    /// </summary>
    North = 0,
    /// <summary>
    /// X + 1
    /// </summary>
    East = 1,
    /// <summary>
    /// Y + 1
    /// </summary>
    South = 2,
    /// <summary>
    /// X - 1
    /// </summary>
    West = 4,
}

[Flags]
public enum TurretDirection
{
    North = 1,
    East = 2,
    South = 4,
    West = 8,
    NorthEast = North | East,
    NorthWest = North | West,
    SouthEast = South | East,
    SouthWest = South | West
}