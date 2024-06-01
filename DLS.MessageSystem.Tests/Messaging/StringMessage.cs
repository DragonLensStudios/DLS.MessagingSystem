namespace DLS.MessageSystem.Tests.Messaging;

[Serializable]
public struct StringMessage
{
    public string Message { get; set; }

    public StringMessage(string message)
    {
        Message = message;
    }
}