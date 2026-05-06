using TankDestroyer.API;

namespace TankDestroyer.Engine;

public class Tank : ITank
{
    public Tank(int owner)
    {
        OwnerId = owner;
    }

    public int X { get; set; }
    public int Y { get; set; }
    public int Health { get; set; } = 100;
    public int OwnerId { get; set; }
    public int Ammo { get; set; }
    public int MaxAmmo { get; set; }
    public TurretDirection TurretDirection { get; set; } = TurretDirection.North;
    public bool Destroyed { get; set; }
    public bool Fired { get; set; }


    public Tank Clone()
    {
        return new Tank(OwnerId)
        {
            X = X,
            Y = Y,
            Health = Health,
            TurretDirection = TurretDirection,
            Destroyed = Destroyed,
            OwnerId = OwnerId,
            Fired = Fired,
            Ammo = Ammo,
            MaxAmmo = MaxAmmo
        };
    }

    public void TakeDamage(int amount)
    {
        Health -= amount;
        Health = Math.Clamp(Health, 0, 100);
    }
}