using TankDestroyer.API;
using TankDestroyer.API.Objects;

namespace TankDestroyer.Engine.Objects;

public class MunitionBox : IMunitionBox
{
    public MunitionBox(int x, int y, int amount = 10)
    {
        X = x;
        Y = y;
        Amount = amount;
        Id = _globalId++;
    }

    private static uint _globalId = 0;

    public uint Id { get; set; }
    public int Amount { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public MunitionBox Clone() => new MunitionBox(X, Y, Amount) { Id = Id };
    public bool IsPickedUp { get; private set; } =  false;

    public void PickUpBy(Tank tank)
    {
        if (!(tank.X == X && tank.Y == Y))
        {
            return;
        }

        var maximumAdd = tank.MaxAmmo - tank.Ammo;

        if (Amount > maximumAdd)
        {
            tank.Ammo += maximumAdd;
        }
        else
        {
            tank.Ammo += Amount;
        }
        
        IsPickedUp = true;
    }
}