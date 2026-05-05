using TankDestroyer.API.Objects;

namespace TankDestroyer.Engine.Objects;

public class Munition : IMunitionBox
{
    public int Amount { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
}