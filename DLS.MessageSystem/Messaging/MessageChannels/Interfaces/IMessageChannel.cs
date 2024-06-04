namespace DLS.MessageSystem.Messaging.MessageChannels.Interfaces
{
    /// <summary>
    ///  Represents a message channel.
    ///  A message channel is a way to group messages together.
    ///  This interface is used to create custom message channels that are not part of the default message channels.
    ///  Custom message channels can be created by extending this interface.
    ///  For example, a custom message channel could be created for a specific application that is not part of the default message channels.
    ///  This custom message channel could then be used to send messages to that specific application.
    /// </summary>
    public interface IMessageChannel
    {
        /// <summary>
        ///  The name of the message channel.
        /// </summary>
        string Name { get; }
    }
}