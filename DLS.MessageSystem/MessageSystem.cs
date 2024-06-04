using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DLS.MessageSystem.Messaging.MessageChannels.Interfaces;
using Newtonsoft.Json;

namespace DLS.MessageSystem
{
    /// <summary>
    ///   A robust message system for sending messages between different parts of an application.
    ///  using a publish-subscribe pattern.
    ///  Messages can be sent immediately or queued for later processing.
    ///  Messages can be sent synchronously or asynchronously.
    ///  Messages can be sent to a specific channel or broadcasted to all channels.
    ///  Messages can be serialized to JSON or binary and deserialized back to their original form.
    ///  Messages can be prioritized for processing.
    ///  Messages can be handled by multiple handlers.
    ///  Messages can be filtered by type.
    ///  Messages can be sent to multiple channels.
    ///  Messages can be sent to multiple handlers.
    ///  Messages can be sent to multiple channels and handlers.
    ///  Messages can be sent to multiple channels and handlers with different priorities.
    ///  Messages can be sent to multiple channels and handlers with different priorities asynchronously.
    /// </summary>
    public static class MessageSystem
    {
        /// <summary>
        ///  A message envelope that wraps a message and its type.
        /// </summary>
        public interface IMessageEnvelope
        {
            Type MessageType { get; }
        }

        /// <summary>
        ///  A message envelope that wraps a message and its type.
        ///  The message is of type T.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        public interface IMessageEnvelope<out T> : IMessageEnvelope
        {
            T? Message { get; }
        }

        /// <summary>
        ///  A message envelope that wraps a message and its type.
        ///  The message is of type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class MessageEnvelope<T> : IMessageEnvelope<T?>
        {
            public T? Message { get; }
            public Type MessageType { get; } = typeof(T);

            public MessageEnvelope(T? message)
            {
                Message = message;
            }
        }

        /// <summary>
        ///  A Message Wrapper that sends a <see cref="IMessageEnvelope{T}"/>
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

        /// <summary>
        ///  The message manager that handles messages.
        ///  Messages can be sent immediately or queued for later processing.
        ///  Messages can be sent synchronously or asynchronously.
        ///  Messages can be sent to a specific channel or broadcasted to all channels.
        ///  Messages can be serialized to JSON or binary and deserialized back to their original form.
        ///  Messages can be prioritized for processing.
        ///  Messages can be handled by multiple handlers.
        ///  Messages can be filtered by type.
        ///  Messages can be sent to multiple channels.
        ///  Messages can be sent to multiple handlers.
        ///  Messages can be sent to multiple channels and handlers.
        ///  Messages can be sent to multiple channels and handlers with different priorities.
        /// </summary>
        public static class MessageManager
        {
            // Channels, handlers, message queues, and priorities
            private static readonly Dictionary<IMessageChannel, List<Delegate>> _channels = new();
            private static readonly ConcurrentDictionary<IMessageChannel, ConcurrentQueue<IMessageEnvelope>> _messageQueues = new();
            private static readonly ConcurrentDictionary<IMessageChannel, int> _priorities = new();

            // Lock object for thread safety
            private static readonly object _lockObject = new();
            
            /// <summary>
            ///  Process messages based on priority using multiple threads.
            ///  Messages are processed in order of priority.
            ///  Messages are processed synchronously.
            /// </summary>
            public static void ProcessMessages()
            {
                var channels = _priorities.Keys.OrderBy(key => _priorities[key]).ToList();

                Parallel.ForEach(channels, ProcessChannelMessages);
            }

            /// <summary>
            ///  Process Channel Messages gets the handlers for a channel and processes the messages.
            /// </summary>
            /// <param name="channel"></param>
            private static void ProcessChannelMessages(IMessageChannel channel)
            {
                while (_messageQueues[channel].TryDequeue(out var envelope))
                {
                    try
                    {
                        var handlers = GetChannelHandlers(channel);

                        foreach (var handler in handlers)
                        {
                            var action = (Action<IMessageEnvelope>)handler;
                            action(envelope);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing message of type {envelope.MessageType.Name} on channel {channel}: {ex.Message}");
                    }
                }
            }
            
            
            /// <summary>
            ///  Process messages asynchronously based on priority using multiple tasks.
            ///  Messages are processed in order of priority.
            /// </summary>
            public static async Task ProcessMessagesAsync()
            {
                var channels = _priorities.Keys.OrderBy(key => _priorities[key]).ToList();

                var processTasks = channels.Select(ProcessChannelMessagesAsync);

                await Task.WhenAll(processTasks);
            }

            /// <summary>
            ///  Process Channel Messages asynchronously gets the handlers for a channel and processes the messages.
            /// </summary>
            /// <param name="channel"><see cref="IMessageChannel"/></param>
            private static async Task ProcessChannelMessagesAsync(IMessageChannel channel)
            {
                while (_messageQueues[channel].TryDequeue(out var envelope))
                {
                    try
                    {
                        var handlers = GetChannelHandlers(channel);

                        var handlerTasks = handlers.Select(handler =>
                        {
                            var action = (Action<IMessageEnvelope>)handler;
                            return Task.Run(() => action(envelope));
                        });

                        await Task.WhenAll(handlerTasks);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing message of type {envelope.MessageType.Name} on channel {channel}: {ex.Message}");
                    }
                }
            }

            /// <summary>
            ///  Send a message to be processed immediately.
            ///  Messages are processed synchronously.
            /// </summary>
            /// <param name="channel"><see cref="IMessageChannel"/></param>
            /// <param name="message"><see cref="message"/></param>
            /// <typeparam name="T">Type</typeparam>
            /// <exception cref="InvalidOperationException"><see cref="InvalidOperationException"/></exception>
            public static void SendImmediate<T>(IMessageChannel channel, T message)
            {
                // Check if the channel has handlers registered
                if (!_channels.ContainsKey(channel))
                {
                    // Throw an exception if no handlers are registered
                    throw new InvalidOperationException($"No handlers registered for channel {channel}.");
                }

                // Create a message envelope for the message of type T
                var envelope = new MessageEnvelope<T>(message);

                // Get the handlers for the channel and process the message synchronously
                var handlers = GetChannelHandlers(channel);

                foreach (var handler in handlers)
                {
                    var action = (Action<IMessageEnvelope>)handler;
                    action(envelope);
                }
            }         
            
            /// <summary>
            ///  Send a message to be processed immediately.
            ///  Messages are processed synchronously.
            /// </summary>
            /// <param name="message"><see cref="message"/></param>
            /// <param name="channels"><see cref="IMessageChannel"/></param>
            /// <typeparam name="T">Type</typeparam>
            public static void SendImmediate<T>(T message, params IMessageChannel[] channels)
            {
                // Send the message to each channel immediately
                foreach (var channel in channels)
                {
                    SendImmediate(channel, message);
                }
            }
            
            /// <summary>
            ///  Send a message to be processed later Synchronously.
            ///  Messages are processed Synchronously.
            /// </summary>
            /// <param name="channel"><see cref="IMessageChannel"/></param>
            /// <param name="message"><see cref="message"/></param>
            /// <typeparam name="T">Type</typeparam>
            /// <exception cref="InvalidOperationException"><see cref="InvalidOperationException"/></exception>
            public static void Send<T>(IMessageChannel channel, T message)
            {
                // Check if the channel has handlers registered for it
                if (!_channels.ContainsKey(channel))
                {
                    throw new InvalidOperationException($"No handlers registered for channel {channel}.");
                }

                // Create a message envelope for the message of type T and enqueue it
                var envelope = new MessageEnvelope<T>(message);
                _messageQueues[channel].Enqueue(envelope);

                Console.WriteLine($"Message of type {typeof(T).Name} queued for channel {channel}");
            }

            /// <summary>
            ///  Send a message to be processed later Synchronously.
            ///  Messages are processed Synchronously.
            /// </summary>
            /// <param name="message"><see cref="message"/></param>
            /// <param name="channels"><see cref="IMessageChannel"/></param>
            /// <typeparam name="T">Type</typeparam>
            public static void Send<T>(T message, params IMessageChannel[] channels)
            {
                // Send the message to each channel
                // Enqueue the message for later processing
                foreach (var channel in channels)
                {
                    Send(channel, message);
                }
            }
            
            /// <summary>
            ///  Broadcast a message to be processed immediately.
            ///  Messages are processed synchronously.
            ///  Broadcasts the message to all channels.
            /// </summary>
            /// <param name="message"><see cref="message"/></param>
            /// <typeparam name="T"></typeparam>
            public static void BroadcastImmediate<T>(T message)
            {
                // Get the list of channels
                var channels = _channels.Keys.ToList();

                // Broadcast the message to each channel immediately
                // Process the message synchronously
                Parallel.ForEach(channels, channel =>
                {
                    SendImmediate(channel, message);
                });
#if DEBUG
                Console.WriteLine($"Message of type {typeof(T).Name} broadcasted immediately");
#endif                
            }

            // Broadcast a message to be processed later
            
            /// <summary>
            ///  Broadcast a message to be processed later.
            ///  Messages are processed synchronously.
            ///  Broadcasts the message to all channels.
            /// </summary>
            /// <param name="message"><see cref="message"/></param>
            /// <typeparam name="T">Type</typeparam>
            public static void Broadcast<T>(T message)
            {
                // Get the list of channels and broadcast the message to each channel
                var channels = _channels.Keys.ToList();

                // Broadcast the message to each channel synchronously
                Parallel.ForEach(channels, channel =>
                {
                    Send(channel, message);
                });
                
#if DEBUG
                // Have a debug message to show that the message was broadcasted when in debug mode
                Console.WriteLine($"Message of type {typeof(T).Name} broadcasted for later processing");
#endif
            }
            
            /// <summary>
            ///  Register a handler for a channel with a specific priority.
            ///  The handler is of type <see cref="Action{IMessageEnvelope}"/>
            ///  The channel is of type <see cref="IMessageChannel"/> 
            /// </summary>
            /// <param name="channel"><see cref="IMessageChannel"/></param>
            /// <param name="message"><see cref="message"/></param>
            /// <typeparam name="T">Type</typeparam>
            /// <exception cref="InvalidOperationException"><see cref="InvalidOperationException"/></exception>
            public static async Task SendImmediateAsync<T>(IMessageChannel channel, T message)
            {
                // Check if the channel has handlers registered for it
                if (!_channels.ContainsKey(channel))
                {
                    // Throw an exception if no handlers are registered
                    throw new InvalidOperationException($"No handlers registered for channel {channel}.");
                }

                // Create a message envelope for the message of type T
                var envelope = new MessageEnvelope<T>(message);

                // Get the handlers for the channel
                var handlers = GetChannelHandlers(channel);

                // Send the message to each handler asynchronously and process the message immediately
                // Process the message asynchronously
                var sendTasks = handlers.Select(handler =>
                {
                    var action = (Action<IMessageEnvelope>)handler;
                    return Task.Run(() => action(envelope));
                });

                // Wait for all the send tasks to complete
                await Task.WhenAll(sendTasks);
            }

            /// <summary>
            ///  Send a message asynchronously to be processed immediately.
            ///  Messages are processed asynchronously.
            /// </summary>
            /// <param name="message"><see cref="message"/></param>
            /// <param name="channels"><see cref="IMessageChannel"/></param>
            /// <typeparam name="T">Type</typeparam>
            public static async Task SendImmediateAsync<T>(T message, params IMessageChannel[] channels)
            {
                // Send the message to each channel asynchronously and process the message asynchronously immediately
                var sendTasks = channels.Select(channel => SendImmediateAsync(channel, message));
                // Wait for all the send tasks to complete
                await Task.WhenAll(sendTasks);
            }

            
            /// <summary>
            ///  Send a message asynchronously to be processed later.
            ///  Messages are processed asynchronously.
            /// </summary>
            /// <param name="channel"><see cref="IMessageChannel"/></param>
            /// <param name="message"><see cref="message"/></param>
            /// <typeparam name="T">Type</typeparam>
            /// <exception cref="InvalidOperationException"></exception>
            public static async Task SendAsync<T>(IMessageChannel channel, T message)
            {
                // Check if the channel has handlers registered for it
                if (!_channels.ContainsKey(channel))
                {
                    // Throw an exception if no handlers are registered
                    throw new InvalidOperationException($"No handlers registered for channel {channel}.");
                }

                // Create a message envelope for the message of type T and enqueue it
                var envelope = new MessageEnvelope<T>(message);
                _messageQueues[channel].Enqueue(envelope);
                
#if DEBUG
                // Have a debug message to show that the message was broadcasted when in debug mode
                Console.WriteLine($"Message of type {typeof(T).Name} queued for channel {channel}");
#endif
                // Wait for the task to complete
                await Task.CompletedTask;
            }

            /// <summary>
            ///  Send a message asynchronously to be processed later.
            ///  Messages are processed asynchronously.
            /// </summary>
            /// <param name="message"><see cref="message"/></param>
            /// <param name="channels"><see cref="IMessageChannel"/></param>
            /// <typeparam name="T">Type</typeparam>
            public static async Task SendAsync<T>(T message, params IMessageChannel[] channels)
            {
                // Send the message to each channel asynchronously
                var sendTasks = channels.Select(channel => SendAsync(channel, message));
                // Wait for all the send tasks to complete
                await Task.WhenAll(sendTasks);
            }
            
            /// <summary>
            ///  Broadcast a message asynchronously to be processed immediately.
            ///  Messages are processed asynchronously.
            ///  Broadcasts the message to all channels.
            ///  The message is of type T.
            /// </summary>
            /// <param name="message">Implements <see cref="IMessageChannel"/></param>
            /// <typeparam name="T">Type</typeparam>
            public static async Task BroadcastImmediateAsync<T>(T message)
            {
                // Get the list of channels
                var channels = _channels.Keys.ToList();
                // Broadcast the message to each channel asynchronously
                var broadcastTasks = channels.Select(channel => SendImmediateAsync(channel, message));
                // Wait for all the broadcast tasks to complete
                await Task.WhenAll(broadcastTasks);
#if DEBUG
                // Have a debug message to show that the message was broadcasted when in debug mode
                // This is ran asynchronously so it may not show up in the console immediately
                Console.WriteLine($"Message of type {typeof(T).Name} broadcasted immediately");
#endif
            }
            
            /// <summary>
            ///  Broadcast a message asynchronously to be processed later.
            ///  Messages are processed asynchronously.
            ///  Broadcasts the message to all channels.
            ///  The message is of type T.
            /// </summary>
            /// <param name="message">Implimenting <see cref="IMessageChannel"/> </param>
            /// <typeparam name="T">Type</typeparam>
            public static async Task BroadcastAsync<T>(T message)
            {
                // Get the list of channels and broadcast the message to each channel
                var channels = _channels.Keys.ToList();

                // Broadcast the message to each channel asynchronously
                // Process the message asynchronously
                var broadcastTasks = channels.Select(channel => SendAsync(channel, message));

                // Wait for all the broadcast tasks to complete
                await Task.WhenAll(broadcastTasks);

#if DEBUG
                // Have a debug message to show that the message was broadcasted when in debug mode
                // This is ran asynchronously so it may not show up in the console immediately
                Console.WriteLine($"Message of type {typeof(T).Name} broadcasted for later processing");
#endif
            }
            
            /// <summary>
            ///  Register a handler for a channel with a specific priority.
            ///  The handler is of type <see cref="Action{IMessageEnvelope}"/>
            ///  The channel is of type <see cref="IMessageChannel"/>
            ///  The priority is of type <see cref="int"/>
            ///  The handler is registered for the channel with the specified priority.
            ///  Using a lock object for thread safety.
            /// </summary>
            /// <param name="channel"><see cref="IMessageChannel"/></param>
            /// <param name="handler"><see cref="IMessageEnvelope{T}"/></param>
            /// <param name="priority"><see cref="priority"/></param>
            /// <typeparam name="T">Type</typeparam>
            public static void RegisterForChannel<T>(IMessageChannel channel, Action<IMessageEnvelope> handler, int priority = 0)
            {
                // Register a handler for a channel with a specific priority
                // Lock object for thread safety
                lock (_lockObject)
                {
                    // Check if the channel has handlers registered
                    // If the channel does not have handlers registered
                    // Register the handler for the channel with the specified priority
                    if (!_channels.ContainsKey(channel))
                    {
                        _channels[channel] = new List<Delegate>() { handler };
                        _messageQueues[channel] = new ConcurrentQueue<IMessageEnvelope>();
                        _priorities[channel] = priority;
                    }
                    else
                    {
                        // If the channel has handlers registered
                        // Register the handler for the channel with the specified priority
                        _channels[channel].Add(handler);
                    }
                }

#if DEBUG
                // Have a debug message to show that the handler was registered when in debug mode
                Console.WriteLine($"Handler for type {typeof(T).Name} registered for channel {channel} with priority {priority}");
#endif
            }
            
            /// <summary>
            ///  Register a handler for a channel with a specific priority.
            ///  The handler is of type <see cref="Action{IMessageEnvelope}"/>
            ///  The channel is of type <see cref="IMessageChannel"/>
            ///  The priority is of type <see cref="int"/>
            ///  The handler is registered for the channel with the specified priority.
            ///  Using a lock object for thread safety.
            /// </summary>
            /// <param name="handler"><see cref="IMessageEnvelope{T}"/></param>
            /// <param name="priority"><see cref="priority"/></param>
            /// <param name="channels"><see cref="IMessageChannel"/></param>
            /// <typeparam name="T">Type</typeparam>
            public static void RegisterForChannel<T>(Action<IMessageEnvelope> handler, int priority = 0, params IMessageChannel[] channels)
            {
                // Register a handler for a channel with a specific priority
                foreach (var channel in channels)
                {
                    // Register the handler for the channel with the specified priority
                    // The handler is of type Action<IMessageEnvelope>
                    // The channel is of type IMessageChannel
                    RegisterForChannel<T>(channel, handler, priority);
                }
            }

            
            /// <summary>
            ///  Unregister a handler for a channel.
            ///  The handler is of type <see cref="Action{IMessageEnvelope}"/>
            ///  The channel is of type <see cref="IMessageChannel"/>
            ///  Using a lock object for thread safety.
            ///  The handler is removed from the channel.
            /// </summary>
            /// <param name="channel"><see cref="IMessageChannel"/></param>
            /// <param name="handler"><see cref="IMessageEnvelope{T}"/></param>
            /// <typeparam name="T">Type</typeparam>
            public static void UnregisterForChannel<T>(IMessageChannel channel, Action<IMessageEnvelope> handler)
            {
                // Unregister a handler for a channel
                // The handler is of type Action<IMessageEnvelope>
                // Using a lock object for thread safety
                lock (_lockObject)
                {
                    // Check if the channel has handlers registered
                    if (_channels.ContainsKey(channel))
                    {
                        // Remove the handler from the channel
                        _channels[channel].Remove(handler);
                    }
                }
#if DEBUG
                // Have a debug message to show that the handler was unregistered when in debug mode
                Console.WriteLine($"Handler for type {typeof(T).Name} unregistered for channel {channel}");
#endif
            }
            
            /// <summary>
            ///  Unregister a handler for a channel. The handler is of type <see cref="Action{IMessageEnvelope}"/>
            /// </summary>
            /// <param name="handler"><see cref="IMessageEnvelope/></param>
            /// <param name="channels"><see cref="IMessageChannel"/></param>
            /// <typeparam name="T">Type</typeparam>
            public static void UnregisterForChannel<T>(Action<IMessageEnvelope> handler, params IMessageChannel[] channels)
            {
                foreach (var channel in channels)
                {
                    UnregisterForChannel<T>(channel, handler);
                }
            }
            
            /// <summary>
            ///  Get the handlers for a channel.
            ///  The channel is of type <see cref="IMessageChannel"/>
            ///  The handlers are of type <see cref="Delegate"/>
            ///  The handlers are returned as a list.
            /// </summary>
            /// <param name="channel"><see cref="IMessageChannel"/> </param>
            /// <returns><list type="Delegate"></list></returns>
            private static List<Delegate> GetChannelHandlers(IMessageChannel channel)
            {
                // Get the handlers for a channel
                // The handlers are of type Delegate
                // Using a lock object for thread safety
                lock (_lockObject)
                {
                    // Check if the channel has handlers registered
                    if (_channels.TryGetValue(channel, out var handlers))
                    {
                        return handlers.ToList();
                    }
                }

                // Return an empty list if no handlers are registered
                return new List<Delegate>();
            }
            
            /// <summary>
            ///  Serialize a message to JSON data.
            ///  The message is of type T.
            ///  Using Newtonsoft.Json to serialize the message to JSON data.
            /// </summary>
            /// <param name="message"><see cref="message"/></param>
            /// <typeparam name="T">Type</typeparam>
            /// <returns></returns>
            public static string SerializeMessageToJson<T>(T message)
            {
                // Serialize the message to JSON data
                // Using Newtonsoft.Json to serialize the message to JSON data
                return JsonConvert.SerializeObject(message,Formatting.Indented);
            }

            
            /// <summary>
            ///  Deserialize a message from JSON data. The message is of type T.
            ///  Using Newtonsoft.Json to deserialize the message from JSON data.
            /// </summary>
            /// <param name="json">Json String see: <see cref="Newtonsoft"/></param>
            /// <typeparam name="T">Type</typeparam>
            /// <returns></returns>
            public static T DeserializeMessageFromJson<T>(string json)
            {
                // Deserialize the message from JSON data
                // Return the message of type 
                // Using Newtonsoft.Json to deserialize the message from JSON data
                return JsonConvert.DeserializeObject<T>(json);
            }

            // Serialize message to binary
            /// <summary>
            ///  Serialize a message to binary data.
            ///  The message is of type T.
            /// </summary>
            /// <param name="message"><see cref="message"/></param>
            /// <typeparam name="T">Type</typeparam>
            /// <returns></returns>
            public static byte[] SerializeMessageToBinary<T>(T message)
            {
                // Serialize message to binary
                using (var stream = new System.IO.MemoryStream())
                {
                    // Create a binary formatter and serialize the message
                    var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    // Serialize the message to binary data
                    formatter.Serialize(stream, message);
                    // Return the binary data as a byte array
                    return stream.ToArray();
                }
            }
            
            /// <summary>
            ///  Deserialize a message from binary data.
            ///  The message is of type T.
            /// </summary>
            /// <param name="data"><see cref="data"/></param>
            /// <typeparam name="T">Type</typeparam>
            /// <returns></returns>
            public static T DeserializeMessageFromBinary<T>(byte[] data)
            {
                // Deserialize message from binary
                using (var stream = new System.IO.MemoryStream(data))
                {
                    // Create a binary formatter and deserialize the message
                    var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    // Deserialize the message from binary data
                    return (T)formatter.Deserialize(stream);
                }
            }
        }
    }
}
