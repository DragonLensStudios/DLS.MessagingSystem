using DLS.MessageSystem.Messaging.MessageChannels.Interfaces;

namespace DLS.MessageSystem.Messaging.MessageChannels
{
    /// <summary>
    ///   A custom message channel.
    ///  This is used to create custom message channels that are not part of the default message channels.
    ///  This is useful for creating custom message channels that are specific to a particular application.
    ///  For example, a custom message channel could be created for a specific application that is not part of the default message channels.
    ///  This custom message channel could then be used to send messages to that specific application.
    ///  Custom message channels can be created by extending the <see cref="IMessageChannel"/> interface.
    /// </summary>
    public class CustomChannel : IMessageChannel
    {
        // The name of the custom message channel.
        public string Name { get; }

        // Creates a new custom message channel with the specified name.
        public CustomChannel(string name)
        {
            Name = name;
        }
    }
}