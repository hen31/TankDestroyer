using TankDestroyer.Engine;

namespace TankDestroyer.Console.Configuration;

public class InitialGameObject
{
    public World[] Worlds { get; set; } = [];
    public Type[] Bots { get; set; } = [];
}