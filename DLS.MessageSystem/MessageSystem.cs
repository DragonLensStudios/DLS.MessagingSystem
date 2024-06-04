using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DLS.MessageSystem.Messaging.MessageChannels;
using DLS.MessageSystem.Messaging.MessageChannels.Comparers;
using DLS.MessageSystem.Messaging.MessageChannels.Enums;
using DLS.MessageSystem.Messaging.MessageChannels.Interfaces;
using DLS.MessageSystem.Messaging.MessageWrappers;
using DLS.MessageSystem.Messaging.MessageWrappers.Interfaces;
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
            private static readonly Dictionary<IMessageChannel, List<Action<IMessageEnvelope>>> _channels = new(new MessageChannelComparer());
            private static readonly ConcurrentDictionary<IMessageChannel, ConcurrentQueue<IMessageEnvelope>> _messageQueues = new(new MessageChannelComparer());
            private static readonly ConcurrentDictionary<IMessageChannel, int> _priorities = new(new MessageChannelComparer());

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
            /// <param name="channel"><see cref="MessageChannels"/></param>
            /// <param name="message"><see cref="message"/></param>
            /// <typeparam name="T">Type</typeparam>
            public static void SendImmediate<T>(MessageChannels channel, T message)
            {
                // Send the message to the channel
                // Wrap the channel in a default message channel
                SendImmediate(new DefaultMessageChannel(channel), message);
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
            ///  Send a message to be processed immediately.
            ///  Messages are processed synchronously.
            /// </summary>
            /// <param name="message"></param>
            /// <param name="channels"></param>
            /// <typeparam name="T"></typeparam>
            public static void SendImmediate<T>(T message, params MessageChannels[] channels)
            {
                // Send the message to each channel immediately
                foreach (var channel in channels)
                {
                    // Wrap the channel in a default message channel
                    SendImmediate(new DefaultMessageChannel(channel), message);
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
            /// <param name="channel"><see cref="MessageChannels"/></param>
            /// <param name="message"><see cref="message"/></param>
            /// <typeparam name="T">Type</typeparam>
            public static void Send<T>(MessageChannels channel, T message)
            {
                // Send the message to the channel
                // Wrap the channel in a default message channel
                Send(new DefaultMessageChannel(channel), message);
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
            ///  Send a message to be processed later Synchronously.
            ///  Messages are processed Synchronously.
            /// </summary>
            /// <param name="message"><see cref="message"/></param>
            /// <param name="channels"><see cref="MessageChannels"/></param>
            /// <typeparam name="T">Type</typeparam>
            public static void Send<T>(T message, params MessageChannels[] channels)
            {
                // Send the message to each channel
                // Enqueue the message for later processing
                foreach (var channel in channels)
                {
                    // Wrap the channel in a default message channel
                    Send(new DefaultMessageChannel(channel), message);
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
            /// <param name="channel"><see cref="MessageChannels"/></param>
            /// <param name="message"><see cref="message"/></param>
            /// <typeparam name="T">Type</typeparam>
            public static async Task SendImmediateAsync<T>(MessageChannels channel, T message)
            {
                // Send the message to the channel asynchronously
                // Wrap the channel in a default message channel
                await SendImmediateAsync(new DefaultMessageChannel(channel), message);
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
            ///  Send a message asynchronously to be processed immediately.
            ///  Messages are processed asynchronously.
            /// </summary>
            /// <param name="message"><see cref="message"/></param>
            /// <param name="channels"><see cref="MessageChannels"/></param>
            /// <typeparam name="T">Type</typeparam>
            public static async Task SendImmediateAsync<T>(T message, params MessageChannels[] channels)
            {
                // Send the message to each channel asynchronously and process the message asynchronously immediately
                var sendTasks = channels.Select(channel => SendImmediateAsync(new DefaultMessageChannel(channel), message));
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
            /// Messages are processed asynchronously.
            /// </summary>
            /// <param name="channel"><see cref="MessageChannels"/></param>
            /// <param name="message"><see cref="message"/></param>
            /// <typeparam name="T">Type</typeparam>
            public static async Task SendAsync<T>(MessageChannels channel, T message)
            {
                // Send the message to the channel asynchronously
                // Wrap the channel in a default message channel
                await SendAsync(new DefaultMessageChannel(channel), message);
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
            ///  Send a message asynchronously to be processed later.
            ///  Messages are processed asynchronously.
            /// </summary>
            /// <param name="message"><see cref="message"/></param>
            /// <param name="channels"><see cref="MessageChannels"/></param>
            /// <typeparam name="T">Type</typeparam>
            public static async Task SendAsync<T>(T message, params MessageChannels[] channels)
            {
                // Send the message to each channel asynchronously
                var sendTasks = channels.Select(channel => SendAsync(new DefaultMessageChannel(channel), message));
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
                    if (channel == null || string.IsNullOrWhiteSpace(channel.Name))
                    {
                        throw new ArgumentException("Channel must have a valid name.");
                    }
                    // Check if the channel has handlers registered
                    // If the channel does not have handlers registered
                    // Register the handler for the channel with the specified priority
                    if (!_channels.ContainsKey(channel))
                    {
                        _channels[channel] = new List<Action<IMessageEnvelope>> { handler };
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
            /// Register a handler for a channel with a specific priority.
            /// The handler is of type <see cref="Action{IMessageEnvelope}"/>.
            /// The channel is of type <see cref="MessageChannels"/>.
            /// The priority is of type <see cref="int"/>.
            /// The handler is registered for the channel with the specified priority.
            /// Using a lock object for thread safety.
            /// </summary>
            /// <typeparam name="T">The type of the message.</typeparam>
            /// <param name="channel">The channel to register the handler for.</param>
            /// <param name="handler">The handler to register.</param>
            /// <param name="priority">The priority of the handler.</param>
            public static void RegisterForChannel<T>(MessageChannels channel, Action<IMessageEnvelope> handler, int priority = 0)
            {
                RegisterForChannel<T>(new DefaultMessageChannel(channel), handler, priority);
            }
            
            /// <summary>
            /// Register a handler for multiple channels with a specific priority.
            /// The handler is of type <see cref="Action{IMessageEnvelope}"/>.
            /// The channels are of type <see cref="MessageChannels"/>.
            /// The priority is of type <see cref="int"/>.
            /// The handler is registered for the channels with the specified priority.
            /// Using a lock object for thread safety.
            /// </summary>
            /// <typeparam name="T">The type of the message.</typeparam>
            /// <param name="handler">The handler to register.</param>
            /// <param name="priority">The priority of the handler.</param>
            /// <param name="channels">The channels to register the handler for.</param>
            public static void RegisterForChannel<T>(Action<IMessageEnvelope> handler, int priority = 0, params MessageChannels[] channels)
            {
                foreach (var channel in channels)
                {
                    RegisterForChannel<T>(new DefaultMessageChannel(channel), handler, priority);
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
                    if (channel == null || string.IsNullOrWhiteSpace(channel.Name))
                    {
                        throw new ArgumentException("Channel must have a valid name.");
                    }
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
            /// Unregister a handler for a channel.
            /// The handler is of type <see cref="Action{IMessageEnvelope}"/>.
            /// The channel is of type <see cref="MessageChannels"/>.
            /// Using a lock object for thread safety.
            /// The handler is removed from the channel.
            /// </summary>
            /// <typeparam name="T">The type of the message.</typeparam>
            /// <param name="channel">The channel to unregister the handler from.</param>
            /// <param name="handler">The handler to unregister.</param>
            public static void UnregisterForChannel<T>(MessageChannels channel, Action<IMessageEnvelope> handler)
            {
                UnregisterForChannel<T>(new DefaultMessageChannel(channel), handler);
            }
            
            /// <summary>
            /// Unregister a handler for multiple channels.
            /// The handler is of type <see cref="Action{IMessageEnvelope}"/>.
            /// The channels are of type <see cref="MessageChannels"/>.
            /// Using a lock object for thread safety.
            /// The handler is removed from the channels.
            /// </summary>
            /// <typeparam name="T">The type of the message.</typeparam>
            /// <param name="handler">The handler to unregister.</param>
            /// <param name="channels">The channels to unregister the handler from.</param>
            public static void UnregisterForChannel<T>(Action<IMessageEnvelope> handler, params MessageChannels[] channels)
            {
                foreach (var channel in channels)
                {
                    UnregisterForChannel<T>(new DefaultMessageChannel(channel), handler);
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
            private static List<Action<IMessageEnvelope>> GetChannelHandlers(IMessageChannel channel)
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
                return new List<Action<IMessageEnvelope>>();
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
        }
    }
}
