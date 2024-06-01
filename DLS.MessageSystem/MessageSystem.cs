using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DLS.MessageSystem.Messaging;
using Newtonsoft.Json;

namespace DLS.MessageSystem
{
    public static class MessageSystem
    {
        public interface IMessageEnvelope
        {
            Type MessageType { get; }
        }

        public interface IMessageEnvelope<out T> : IMessageEnvelope
        {
            T? Message { get; }
        }

        public class MessageEnvelope<T> : IMessageEnvelope, IMessageEnvelope<T?>
        {
            public T? Message { get; private set; }
            public Type MessageType { get; private set; } = typeof(T);

            public MessageEnvelope(T? message)
            {
                Message = message;
            }
        }

        public static T? Message<T>(this IMessageEnvelope? envelope) where T : struct
        {
            IMessageEnvelope<T>? e = envelope as IMessageEnvelope<T>;
            return e?.Message;
        }

        public static class MessageManager
        {
            private static readonly Dictionary<MessageChannels, List<Delegate>> _channels = new Dictionary<MessageChannels, List<Delegate>>();
            private static readonly ConcurrentDictionary<MessageChannels, ConcurrentQueue<IMessageEnvelope>> _messageQueues = new ConcurrentDictionary<MessageChannels, ConcurrentQueue<IMessageEnvelope>>();
            private static readonly ConcurrentDictionary<MessageChannels, int> _priorities = new ConcurrentDictionary<MessageChannels, int>();

            private static readonly object _lockObject = new object();

            // Process messages based on priority using multiple threads
            public static void ProcessMessages()
            {
                var channels = _priorities.Keys.OrderBy(key => _priorities[key]).ToList();

                Parallel.ForEach(channels, ProcessChannelMessages);
            }

            private static void ProcessChannelMessages(MessageChannels channel)
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
            
            // Process messages asynchronously based on priority using multiple tasks
            public static async Task ProcessMessagesAsync()
            {
                var channels = _priorities.Keys.OrderBy(key => _priorities[key]).ToList();

                var processTasks = channels.Select(ProcessChannelMessagesAsync);

                await Task.WhenAll(processTasks);
            }

            private static async Task ProcessChannelMessagesAsync(MessageChannels channel)
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

            // Send a message immediately
            public static void SendImmediate<T>(MessageChannels channel, T message)
            {
                if (!_channels.ContainsKey(channel))
                {
                    throw new InvalidOperationException($"No handlers registered for channel {channel}.");
                }

                var envelope = new MessageEnvelope<T>(message);

                var handlers = GetChannelHandlers(channel);

                foreach (var handler in handlers)
                {
                    var action = (Action<IMessageEnvelope>)handler;
                    action(envelope);
                }
            }            
            public static void SendImmediate<T>(T message, params MessageChannels[] channels)
            {
                foreach (var channel in channels)
                {
                    SendImmediate(channel, message);
                }
            }

            // Send a message to be processed later
            public static void Send<T>(MessageChannels channel, T message)
            {
                if (!_channels.ContainsKey(channel))
                {
                    throw new InvalidOperationException($"No handlers registered for channel {channel}.");
                }

                var envelope = new MessageEnvelope<T>(message);
                _messageQueues[channel].Enqueue(envelope);

                Console.WriteLine($"Message of type {typeof(T).Name} queued for channel {channel}");
            }

            public static void Send<T>(T message, params MessageChannels[] channels)
            {
                foreach (var channel in channels)
                {
                    Send(channel, message);
                }
            }
            
            public static void BroadcastImmediate<T>(T message)
            {
                var channels = _channels.Keys.ToList();

                Parallel.ForEach(channels, channel =>
                {
                    SendImmediate(channel, message);
                });

                Console.WriteLine($"Message of type {typeof(T).Name} broadcasted immediately");
            }

            // Broadcast a message to be processed later
            public static void Broadcast<T>(T message)
            {
                var channels = _channels.Keys.ToList();

                Parallel.ForEach(channels, channel =>
                {
                    Send(channel, message);
                });

                Console.WriteLine($"Message of type {typeof(T).Name} broadcasted for later processing");
            }
            
            public static async Task SendImmediateAsync<T>(MessageChannels channel, T message)
            {
                if (!_channels.ContainsKey(channel))
                {
                    throw new InvalidOperationException($"No handlers registered for channel {channel}.");
                }

                var envelope = new MessageEnvelope<T>(message);

                var handlers = GetChannelHandlers(channel);

                var sendTasks = handlers.Select(handler =>
                {
                    var action = (Action<IMessageEnvelope>)handler;
                    return Task.Run(() => action(envelope));
                });

                await Task.WhenAll(sendTasks);
            }

            public static async Task SendImmediateAsync<T>(T message, params MessageChannels[] channels)
            {
                var sendTasks = channels.Select(channel => SendImmediateAsync(channel, message));
                await Task.WhenAll(sendTasks);
            }

            // Send a message asynchronously to be processed later
            public static async Task SendAsync<T>(MessageChannels channel, T message)
            {
                if (!_channels.ContainsKey(channel))
                {
                    throw new InvalidOperationException($"No handlers registered for channel {channel}.");
                }

                var envelope = new MessageEnvelope<T>(message);
                _messageQueues[channel].Enqueue(envelope);

                Console.WriteLine($"Message of type {typeof(T).Name} queued for channel {channel}");

                await Task.CompletedTask;
            }

            public static async Task SendAsync<T>(T message, params MessageChannels[] channels)
            {
                var sendTasks = channels.Select(channel => SendAsync(channel, message));
                await Task.WhenAll(sendTasks);
            }

            // Broadcast a message immediately asynchronously
            public static async Task BroadcastImmediateAsync<T>(T message)
            {
                var channels = _channels.Keys.ToList();

                var broadcastTasks = channels.Select(channel => SendImmediateAsync(channel, message));

                await Task.WhenAll(broadcastTasks);

                Console.WriteLine($"Message of type {typeof(T).Name} broadcasted immediately");
            }

            // Broadcast a message asynchronously to be processed later
            public static async Task BroadcastAsync<T>(T message)
            {
                var channels = _channels.Keys.ToList();

                var broadcastTasks = channels.Select(channel => SendAsync(channel, message));

                await Task.WhenAll(broadcastTasks);

                Console.WriteLine($"Message of type {typeof(T).Name} broadcasted for later processing");
            }

            // Register a handler for a channel with a specific priority
            public static void RegisterForChannel<T>(MessageChannels channel, Action<IMessageEnvelope> handler, int priority = 0)
            {
                lock (_lockObject)
                {
                    if (!_channels.ContainsKey(channel))
                    {
                        _channels[channel] = new List<Delegate>() { handler };
                        _messageQueues[channel] = new ConcurrentQueue<IMessageEnvelope>();
                        _priorities[channel] = priority;
                    }
                    else
                    {
                        _channels[channel].Add(handler);
                    }
                }

                Console.WriteLine($"Handler for type {typeof(T).Name} registered for channel {channel} with priority {priority}");
            }
            
            public static void RegisterForChannel<T>(Action<IMessageEnvelope> handler, int priority = 0, params MessageChannels[] channels)
            {
                foreach (var channel in channels)
                {
                    RegisterForChannel<T>(channel, handler, priority);
                }
            }


            // Unregister a handler for a channel
            public static void UnregisterForChannel<T>(MessageChannels channel, Action<IMessageEnvelope> handler)
            {
                lock (_lockObject)
                {
                    if (_channels.ContainsKey(channel))
                    {
                        _channels[channel].Remove(handler);
                    }
                }

                Console.WriteLine($"Handler for type {typeof(T).Name} unregistered for channel {channel}");
            }
            
            public static void UnregisterForChannel<T>(Action<IMessageEnvelope> handler, params MessageChannels[] channels)
            {
                foreach (var channel in channels)
                {
                    UnregisterForChannel<T>(channel, handler);
                }
            }

            // Get the handlers for a channel
            private static List<Delegate> GetChannelHandlers(MessageChannels channel)
            {
                lock (_lockObject)
                {
                    if (_channels.TryGetValue(channel, out var handlers))
                    {
                        return handlers.ToList();
                    }
                }

                return new List<Delegate>();
            }
            
            // Serialize message to JSON
            public static string SerializeMessageToJson<T>(T message)
            {
                return JsonConvert.SerializeObject(message,Formatting.Indented);
            }

            // Deserialize message from JSON
            public static T DeserializeMessageFromJson<T>(string json)
            {
                return JsonConvert.DeserializeObject<T>(json);
            }

            // Serialize message to binary
            public static byte[] SerializeMessageToBinary<T>(T message)
            {
                using (var stream = new System.IO.MemoryStream())
                {
                    var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    formatter.Serialize(stream, message);
                    return stream.ToArray();
                }
            }

            // Deserialize message from binary
            public static T DeserializeMessageFromBinary<T>(byte[] data)
            {
                using (var stream = new System.IO.MemoryStream(data))
                {
                    var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    return (T)formatter.Deserialize(stream);
                }
            }
        }
    }
}
