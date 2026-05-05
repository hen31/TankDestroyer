using TankDestroyer.API.Objects;

namespace TankDestroyer.Engine.Objects;

public class MunitionBox : IMunitionBox
{
    public MunitionBox(int x, int y, int amount = 10)
    {
        X = x;
        Y = y;
        Amount = amount;
        _globalId++;
    }

    private static uint _globalId = 0;

    public uint Id { get; set; }
    public int Amount { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public MunitionBox Clone() => new MunitionBox(X, Y, Amount) { Id = Id };
}