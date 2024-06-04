using System;
using DLS.MessageSystem.Messaging.MessageWrappers.Interfaces;

namespace DLS.MessageSystem.Messaging.MessageWrappers
{
    /// <summary>
    /// Represents a message envelope that wraps a message and its type.
    /// The message is of type T.
    /// </summary>
    /// <typeparam name="T">The type of the message.</typeparam>
    public class MessageEnvelope<T> : IMessageEnvelope<T?>
    {
        /// <summary>
        /// Gets the message.
        /// </summary>
        public T? Message { get; }

        /// <summary>
        /// Gets the type of the message.
        /// </summary>
        public Type MessageType { get; } = typeof(T);

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageEnvelope{T}"/> class.
        /// </summary>
        /// <param name="message">The message to wrap.</param>
        public MessageEnvelope(T? message)
        {
            Message = message;
        }
    }
}