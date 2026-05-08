using TankDestroyer.Engine;

namespace TankDestroyer.Console.Configuration;

public interface IConfigLoader
{
    public InitialGameObject? LoadConfig();
}