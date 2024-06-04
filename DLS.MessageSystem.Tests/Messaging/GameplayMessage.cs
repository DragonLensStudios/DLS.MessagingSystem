namespace DLS.MessageSystem.Tests.Messaging;

[Serializable]
public struct GameplayMessage
{
    public string Message { get; }

    public GameplayMessage(string message)
    {
        Message = message;
    }
}