using System.Collections.Generic;
using DLS.MessageSystem.Messaging.MessageChannels.Interfaces;

namespace DLS.MessageSystem.Messaging.MessageChannels.Comparers
{
    // Implementing equality based on the channel name
    /// <summary>
    ///  A comparer for <see cref="IMessageChannel"/> that compares based on the channel name.
    ///  This is useful for comparing message channels based on their name.
    ///  This can be used to compare message channels in a collection.
    ///  For example, this can be used to compare message channels in a list or dictionary.
    ///  This can be used to compare message channels in a collection to see if they are equal.
    /// </summary>
    public class MessageChannelComparer : IEqualityComparer<IMessageChannel>
    {
        // Determines if two message channels are equal based on their name.
        public bool Equals(IMessageChannel x, IMessageChannel y)
        {
            return x.Name == y.Name;
        }

        // Gets the hash code of the message channel based on its name.
        public int GetHashCode(IMessageChannel channel)
        {
            return channel.Name.GetHashCode();
        }
    }
}