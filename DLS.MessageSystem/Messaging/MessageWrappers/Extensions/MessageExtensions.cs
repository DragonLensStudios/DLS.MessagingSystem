using DLS.MessageSystem.Messaging.MessageWrappers.Interfaces;

namespace DLS.MessageSystem.Messaging.MessageWrappers.Extensions
{
    /// <summary>
    ///  A set of extension methods for <see cref="IMessageEnvelope"/> objects
    ///  that allow for easy unwrapping of messages.
    ///  This class cannot be inherited.
    /// </summary>
    public static class MessageExtensions
    {
        /// <summary>
        ///  A Message Wrapper that sends a <see cref="IMessageEnvelope"/>
        /// </summary>
        /// <param name="envelope"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T? Message<T>(this IMessageEnvelope? envelope) where T : struct
        {
            // Wrap the message in an envelope of type T
            IMessageEnvelope<T>? e = envelope as IMessageEnvelope<T>;
            return e?.Message;
        }
    }
}