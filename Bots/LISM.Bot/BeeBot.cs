using TankDestroyer.API;

namespace LISM.Bot;

[Bot("Bee Bot", "Lien", "E9AB17")]
public class BeeBot : IPlayerBot
{
    private Random _random = new();

    public void DoTurn(ITurnContext turnContext)
    {
        var enumValues = Enum.GetValues<TurretDirection>();
        var enumDirectionValues = Enum.GetValues<Direction>();

        turnContext.MoveTank(enumDirectionValues[_random.Next(0, enumDirectionValues.Length)]);
        turnContext.RotateTurret(enumValues[_random.Next(0, enumValues.Length)]);

        turnContext.Fire();
    }
}