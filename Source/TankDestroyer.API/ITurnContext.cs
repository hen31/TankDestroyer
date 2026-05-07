using TankDestroyer.API.Objects;

namespace TankDestroyer.API;

public interface ITurnContext
{
    ITile GetTile(int y, int x);

    int GetMapWidth();
    int GetMapHeight();

    ITank[] GetTanks();

    IBullet[] GetBullets();
    IMunitionBox[] GetAmmoBoxes();
    public ITank Tank { get; set; }
    public void MoveTank(Direction direction);
    public void RotateTurret(TurretDirection direction);
    void Fire();
}