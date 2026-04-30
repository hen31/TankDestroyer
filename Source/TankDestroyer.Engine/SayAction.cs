using TankDestroyer.API;

namespace TankDestroyer.Engine;

public class SayAction : TankAction
{
    public string Message { get; }
    public SayAction(int ownerId, string message) : base(ownerId)
    {
        Message = message;
    }

    internal override bool Execute(Game game)
    {
        game.World.Messages = game.World.Messages.Prepend(new Message(OwnerId, Message)).ToList();
        return true;
    }
}
