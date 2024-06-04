using System;

namespace DLS.MessageSystem.Messaging.MessageWrappers.Interfaces
{
    /// <summary>
    /// Represents a message envelope that wraps a message and its type.
    /// </summary>
    public interface IMessageEnvelope
    {
        /// <summary>
        /// Gets the type of the message.
        /// </summary>
        Type MessageType { get; }
    }
    
    /// <summary>
    /// Represents a message envelope that wraps a message and its type.
    /// The message is of type T.
    /// </summary>
    /// <typeparam name="T">The type of the message.</typeparam>
    public interface IMessageEnvelope<out T> : IMessageEnvelope
    {
        /// <summary>
        /// Gets the message.
        /// </summary>
        T? Message { get; }
    }
}