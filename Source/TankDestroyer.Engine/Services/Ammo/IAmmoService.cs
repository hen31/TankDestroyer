namespace TankDestroyer.Engine.Services.Ammo;

public interface IAmmoService
{
    public int SpawnAmmo(int range);
    public void PickupAmmo(GameTurn turn);
}