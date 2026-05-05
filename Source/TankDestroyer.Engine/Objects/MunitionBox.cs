using TankDestroyer.API.Objects;

namespace TankDestroyer.Engine.Objects;

public class MunitionBox : IMunitionBox
{
    public int Amount { get; set; }
    public int X { get; set; }
    public int Y { get; set; }

    public MunitionBox(int x, int y, int amount = 10)
    {
        Amount = amount;
        X = x;
        Y = y;
    }
}