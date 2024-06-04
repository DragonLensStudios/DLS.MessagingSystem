using DLS.MessageSystem.Messaging.MessageChannels.Interfaces;

namespace DLS.MessageSystem.Messaging.MessageChannels
{
    /// <summary>
    ///  Default implementation of <see cref="IMessageChannel"/>.
    ///  This class is used to represent a message channel.
    ///  A message channel is a way to group messages together.
    /// </summary>
    public class DefaultMessageChannel : IMessageChannel
    {
        /// <summary>
        ///  The message channels. This is used to group messages together.
        /// </summary>
        public MessageChannels.Enums.MessageChannels Channels { get; }
        
        // Creates a new default message channel with the specified message channels.
        public DefaultMessageChannel(MessageChannels.Enums.MessageChannels channels)
        {
            Channels = channels;
        }
        
        // The name of the message channel. 
        public string Name => Channels.ToString();
    }
}