using TankDestroyer.API.Objects;

namespace TankDestroyer.Engine.Objects;

public class MunitionBox(int x, int y, int amount = 10) : IMunitionBox
{
    public int Amount { get; set; } = amount;
    public int X { get; set; } = x;
    public int Y { get; set; } = y;

    public MunitionBox Clone()
    {
        return new MunitionBox(
            X = X,
            Y = Y,
            Amount = Amount
        );
    }
}