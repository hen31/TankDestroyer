namespace TankDestroyer.Engine.Services.Instantiate;

public interface ICollectMapsService
{
    public World[] LoadMaps(string folder);
}