using DLS.MessageSystem.Messaging;
using DLS.MessageSystem.Messaging.MessageChannels;
using DLS.MessageSystem.Messaging.MessageChannels.Enums;
using DLS.MessageSystem.Messaging.MessageChannels.Interfaces;
using DLS.MessageSystem.Tests.Models;

namespace DLS.MessageSystem.Tests.Messaging
{
    
    public class MessagingTests
    {
        protected const string AwesomeTime = "AwesomeTime";
        protected const string TacoString = "Taco";
        
        // Create message channels for the different message types we want to send and receive in the test cases 
        // (e.g. Gameplay, System, UI, etc.)
        // We will use these channels to send messages to specific handlers
        protected IMessageChannel gamePlayChannel = new DefaultMessageChannel(MessageChannels.Gameplay);
        protected IMessageChannel systemChannel = new DefaultMessageChannel(MessageChannels.System);
        protected IMessageChannel uiChannel = new DefaultMessageChannel(MessageChannels.UI);
        protected IMessageChannel awesomeCustomChannel = new CustomChannel(AwesomeTime);
        protected IMessageChannel tacoCustomChannel = new CustomChannel(TacoString);
        
        private TestClass testObject;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            testObject = new TestClass();

            // Register message handlers
            MessageSystem.MessageManager.RegisterForChannel<GameplayMessage>(gamePlayChannel, GameplayMessageHandler);
            MessageSystem.MessageManager.RegisterForChannel<SystemMessage>(systemChannel, SystemMessageHandler);;
            MessageSystem.MessageManager.RegisterForChannel<GameplayMessage>(systemChannel, GamePlayMessageHandler2);
            MessageSystem.MessageManager.RegisterForChannel<GameplayMessage>(tacoCustomChannel, TacoCustomChannel);
            
            MessageSystem.MessageManager.RegisterForChannel<SystemMessage>(AwesomeCustomChannelHandler, 0, awesomeCustomChannel, gamePlayChannel);
            MessageSystem.MessageManager.RegisterForChannel<StringMessage>(StringMessageHandler, 0, gamePlayChannel, systemChannel, uiChannel);
        }
        
        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            
            // Unregister message handlers
            MessageSystem.MessageManager.UnregisterForChannel<GameplayMessage>(gamePlayChannel, GameplayMessageHandler);
            MessageSystem.MessageManager.UnregisterForChannel<SystemMessage>(systemChannel, SystemMessageHandler);;
            MessageSystem.MessageManager.UnregisterForChannel<GameplayMessage>(systemChannel, GamePlayMessageHandler2);
            MessageSystem.MessageManager.UnregisterForChannel<GameplayMessage>(tacoCustomChannel, TacoCustomChannel);

            MessageSystem.MessageManager.UnregisterForChannel<SystemMessage>(AwesomeCustomChannelHandler, awesomeCustomChannel, gamePlayChannel);
            MessageSystem.MessageManager.UnregisterForChannel<StringMessage>(StringMessageHandler, gamePlayChannel, systemChannel, uiChannel);
        }

        private void TacoCustomChannel(MessageSystem.IMessageEnvelope message)
        {
            if (!message.Message<GameplayMessage>().HasValue) return;
            var data = message.Message<GameplayMessage>().Value;
            testObject.StringProp = data.Message;
        }

        private void AwesomeCustomChannelHandler(MessageSystem.IMessageEnvelope message)
        {
            if (!message.Message<SystemMessage>().HasValue) return;
            var data = message.Message<SystemMessage>().Value;
            testObject.intField = data.TestObject.intField;
            testObject.stringField = data.TestObject.stringField;
        }

        private void StringMessageHandler(MessageSystem.IMessageEnvelope message)
        {
            if(!message.Message<StringMessage>().HasValue) return;
            var data = message.Message<StringMessage>().Value;
            testObject.StringProp = data.Message;
        }

        private void GamePlayMessageHandler2(MessageSystem.IMessageEnvelope message)
        {
            if(!message.Message<GameplayMessage>().HasValue) return;
            var data = message.Message<GameplayMessage>().Value;
            testObject.StringProp = data.Message;
        }

        private void SystemMessageHandler(MessageSystem.IMessageEnvelope message)
        {
            if(!message.Message<SystemMessage>().HasValue) return;
            var data = message.Message<SystemMessage>().Value;
            testObject.intField = data.TestObject.intField;
            testObject.stringField = data.TestObject.stringField;
        }

        private void GameplayMessageHandler(MessageSystem.IMessageEnvelope message)
        {
            if(!message.Message<GameplayMessage>().HasValue) return;
            var data = message.Message<GameplayMessage>().Value;
            testObject.stringField = data.Message;
        }

        [Test]
        [Category("Messaging System Tests")]
        [Order(1)]
        public void Message_Send_String_To_Gameplay_Immediate()
        {
            var text = "Test Taco";
            MessageSystem.MessageManager.SendImmediate(gamePlayChannel, new GameplayMessage(text));
            Assert.AreEqual(testObject.stringField, text);
        }
        
        [Test]
        [Category("Messaging System Tests")]
        [Order(2)]
        public void Message_Send_String_To_Multiple_Immediate()
        {
            var text = "Super Taco";
            MessageSystem.MessageManager.SendImmediate(new StringMessage(text), gamePlayChannel, systemChannel, uiChannel);
            Assert.AreEqual(testObject.StringProp, text);
        }

        [Test]
        [Category("Messaging System Tests")]
        [Order(3)]
        public void Message_Send_TestClass_To_System_Immediate()
        {
            testObject.stringField = "Tomato";
            testObject.intField = 1337;
            MessageSystem.MessageManager.SendImmediate(systemChannel, new SystemMessage(testObject));
            Assert.AreEqual("Tomato", testObject.stringField);
            Assert.AreEqual(1337, testObject.intField);
        }

        [Test]
        [Category("Messaging System Tests")]
        [Order(4)]
        public void Message_Send_Gameplay_To_System_Immediate()
        {
            MessageSystem.MessageManager.SendImmediate(systemChannel, new GameplayMessage("Ninjas"));
            Assert.AreEqual("Ninjas", testObject.StringProp);
        }

        [Test]
        [Category("Messaging System Tests")]
        [Order(5)]
        public void Message_Broadcast_All_Channels_Immediate()
        {
            // Act
            MessageSystem.MessageManager.RegisterForChannel<StringMessage>(StringMessageHandler, 0, gamePlayChannel, systemChannel, uiChannel);

            MessageSystem.MessageManager.BroadcastImmediate(new StringMessage("Broadcast Message Immediate"));

            // Assert
            Assert.AreEqual("Broadcast Message Immediate", testObject.StringProp);
        }

        [Test]
        [Category("Messaging System Tests")]
        [Order(6)]
        public void Message_Send_String_To_Gameplay_Queued()
        {
            var text = "Test Message";
            MessageSystem.MessageManager.Send(gamePlayChannel, new GameplayMessage(text));
            MessageSystem.MessageManager.ProcessMessages();
            Assert.AreEqual(testObject.stringField, text);
        }
        
        [Test]
        [Category("Messaging System Tests")]
        [Order(7)]
        public void Message_Send_String_To_Multiple_Queued()
        {
            var text = "Multiple Test Message";
            MessageSystem.MessageManager.Send(new StringMessage(text), gamePlayChannel, systemChannel, uiChannel);
            MessageSystem.MessageManager.ProcessMessages();
            Assert.AreEqual(testObject.StringProp, text);
        }

        [Test]
        [Category("Messaging System Tests")]
        [Order(8)]
        public void Message_Send_TestClass_To_System_Queued()
        {
            testObject.stringField = "Tacos";
            testObject.intField = 420;
            MessageSystem.MessageManager.Send(systemChannel, new SystemMessage(testObject));
            MessageSystem.MessageManager.ProcessMessages();
            Assert.AreEqual("Tacos", testObject.stringField);
            Assert.AreEqual(420, testObject.intField);
        }

        [Test]
        [Category("Messaging System Tests")]
        [Order(9)]
        public void Message_Send_Gameplay_To_System_Queued()
        {
            MessageSystem.MessageManager.Send(systemChannel, new GameplayMessage("Tacos"));
            MessageSystem.MessageManager.ProcessMessages();
            Assert.AreEqual("Tacos", testObject.StringProp);
        }

        [Test]
        [Category("Messaging System Tests")]
        [Order(10)]
        public void Message_Send_Gameplay_To_All()
        {
            MessageSystem.MessageManager.Broadcast(new GameplayMessage("Tacos"));
            MessageSystem.MessageManager.ProcessMessages();
            Assert.AreEqual("Tacos", testObject.StringProp);
        }

        [Test]
        [Category("Messaging System Tests")]
        [Order(11)]
        public void Message_Send_System_To_All()
        {
            testObject.stringField = "Tacos";
            testObject.intField = 420;
            MessageSystem.MessageManager.Broadcast(new SystemMessage(testObject));
            MessageSystem.MessageManager.ProcessMessages();
            Assert.AreEqual("Tacos", testObject.stringField);
            Assert.AreEqual(420, testObject.intField);
        }

        [Test]
        [Category("Messaging System Tests")]
        [Order(12)]
        public void Message_Send_With_Priority()
        {
            // Arrange
            var lowPriorityHandler = new Action<MessageSystem.IMessageEnvelope>(envelope =>
            {
                var message = envelope.Message<StringMessage>().Value;
                testObject.StringProp = message.Message + " Low";
            });

            var highPriorityHandler = new Action<MessageSystem.IMessageEnvelope>(envelope =>
            {
                var message = envelope.Message<StringMessage>().Value;
                testObject.StringProp = message.Message + " High";
            });

            // Act
            MessageSystem.MessageManager.RegisterForChannel<StringMessage>(systemChannel, lowPriorityHandler, 0);
            MessageSystem.MessageManager.RegisterForChannel<StringMessage>(systemChannel, highPriorityHandler, 1);
            MessageSystem.MessageManager.Send(systemChannel, new StringMessage("Test"));
            MessageSystem.MessageManager.ProcessMessages();

            // Assert
            Assert.AreEqual("Test High", testObject.StringProp);
        }

        [Test]
        [Category("Messaging System Tests")]
        [Order(13)]
        public void Message_Unregister_Handler()
        {
            // Arrange
            var handler = new Action<MessageSystem.IMessageEnvelope>(envelope =>
            {
                var message = envelope.Message<StringMessage>().Value;
                testObject.StringProp = message.Message;
            });

            // Act
            MessageSystem.MessageManager.RegisterForChannel<StringMessage>(systemChannel, handler);
            MessageSystem.MessageManager.SendImmediate(systemChannel, new StringMessage("First Message"));
            Assert.AreEqual("First Message", testObject.StringProp);

            MessageSystem.MessageManager.UnregisterForChannel<StringMessage>(systemChannel, handler);
            MessageSystem.MessageManager.SendImmediate(systemChannel, new StringMessage("Second Message"));

            // Assert
            Assert.AreNotEqual("Second Message", testObject.StringProp);
        }

        [Test]
        [Category("Messaging System Tests")]
        [Order(14)]
        public void Message_Broadcast_All_Channels_Queue()
        {
            // Act
            MessageSystem.MessageManager.RegisterForChannel<StringMessage>(StringMessageHandler, 0, gamePlayChannel, systemChannel, uiChannel);

            MessageSystem.MessageManager.Broadcast(new StringMessage("Broadcast Message"));

            // Process messages
            MessageSystem.MessageManager.ProcessMessages();

            // Assert
            Assert.AreEqual("Broadcast Message", testObject.StringProp);
        }

        [Test]
        [Category("Messaging System Tests")]
        [Order(15)]
        public void Message_Send_Non_Existing_Channel()
        {
            // Arrange
            IMessageChannel nonExistingChannel = new CustomChannel("");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => MessageSystem.MessageManager.SendImmediate(nonExistingChannel, new StringMessage("Test")));
        }

        [Test]
        [Category("Messaging System Tests")]
        [Order(16)]
        public void Message_Send_Serialize_Deserialize_JSON()
        {
            // Arrange
            var originalMessage = new StringMessage("Serialize Test");
            var serializedMessage = MessageSystem.MessageManager.SerializeMessageToJson(originalMessage);

            // Act
            var deserializedMessage = MessageSystem.MessageManager.DeserializeMessageFromJson<StringMessage>(serializedMessage);

            // Assert
            Assert.AreEqual(originalMessage.Message, deserializedMessage.Message);
        }

        [Test]
        [Category("Messaging System Tests")]
        [Order(17)]
        public void Message_Send_Serialize_Deserialize_Binary()
        {
            // Arrange
            var originalMessage = new StringMessage("Serialize Test");
            var serializedMessage = MessageSystem.MessageManager.SerializeMessageToBinary(originalMessage);

            // Act
            var deserializedMessage = MessageSystem.MessageManager.DeserializeMessageFromBinary<StringMessage>(serializedMessage);

            // Assert
            Assert.AreEqual(originalMessage.Message, deserializedMessage.Message);
        }
        
        [Test]
        [Category("Messaging System Tests")]
        [Order(18)]
        public async Task Message_Send_String_To_Gameplay_Immediate_Async()
        {
            var text = "Test Async";
            await MessageSystem.MessageManager.SendImmediateAsync(gamePlayChannel, new GameplayMessage(text));
            Assert.AreEqual(testObject.stringField, text);
        }
        
        [Test]
        [Category("Messaging System Tests")]
        [Order(19)]
        public async Task Message_Send_String_To_Multiple_Immediate_Async()
        {
            var text = "Test Multiple Async";
            await MessageSystem.MessageManager.SendImmediateAsync(new GameplayMessage(text), gamePlayChannel, systemChannel, uiChannel);
            Assert.AreEqual(testObject.StringProp, text);
        }

        [Test]
        [Category("Messaging System Tests")]
        [Order(19)]
        public async Task Message_Send_String_To_Multiple_Queued_Async()
        {
            var text = "Test Queued Multiple Async";
            await MessageSystem.MessageManager.SendAsync(new GameplayMessage(text), gamePlayChannel, systemChannel, uiChannel);
            await MessageSystem.MessageManager.ProcessMessagesAsync();
            Assert.AreEqual(testObject.StringProp, text);
        }
 
        [Test]
        [Category("Messaging System Tests")]
        [Order(20)]
        public async Task Message_Send_TestClass_To_System_Immediate_Async()
        {
            testObject.stringField = "Potato";
            testObject.intField = 9999;
            await MessageSystem.MessageManager.SendImmediateAsync(systemChannel, new SystemMessage(testObject));
            Assert.AreEqual("Potato", testObject.stringField);
            Assert.AreEqual(9999, testObject.intField);
        }

        [Test]
        [Category("Messaging System Tests")]
        [Order(21)]
        public async Task Message_Send_Gameplay_To_System_Immediate_Async()
        {
            await MessageSystem.MessageManager.SendImmediateAsync(systemChannel, new GameplayMessage("Pirates"));
            Assert.AreEqual("Pirates", testObject.StringProp);
        }

        [Test]
        [Category("Messaging System Tests")]
        [Order(22)]
        public async Task Message_Broadcast_All_Channels_Immediate_Async()
        {
            // Arrange
            var handler = new Action<MessageSystem.IMessageEnvelope>(envelope =>
            {
                var message = envelope.Message<StringMessage>().Value;
                testObject.StringProp = message.Message;
            });

            // Act
            MessageSystem.MessageManager.RegisterForChannel<StringMessage>(StringMessageHandler, 0, gamePlayChannel, systemChannel, uiChannel);

            await MessageSystem.MessageManager.BroadcastImmediateAsync(new StringMessage("Broadcast Message Immediate Async"));

            // Assert
            Assert.AreEqual("Broadcast Message Immediate Async", testObject.StringProp);
        }

        [Test]
        [Category("Messaging System Tests")]
        [Order(23)]
        public async Task Message_Send_String_To_Gameplay_Queued_Async()
        {
            var text = "Test Message Async";
            await MessageSystem.MessageManager.SendAsync(gamePlayChannel, new GameplayMessage(text));
            await MessageSystem.MessageManager.ProcessMessagesAsync();
            Assert.AreEqual(testObject.stringField, text);
        }

        [Test]
        [Category("Messaging System Tests")]
        [Order(24)]
        public async Task Message_Send_TestClass_To_System_Queued_Async()
        {
            testObject.stringField = "Burrito";
            testObject.intField = 1234;
            await MessageSystem.MessageManager.SendAsync(systemChannel, new SystemMessage(testObject));
            await MessageSystem.MessageManager.ProcessMessagesAsync();
            Assert.AreEqual("Burrito", testObject.stringField);
            Assert.AreEqual(1234, testObject.intField);
        }

        [Test]
        [Category("Messaging System Tests")]
        [Order(25)]
        public async Task Message_Send_Gameplay_To_System_Queued_Async()
        {
            await MessageSystem.MessageManager.SendAsync(systemChannel, new GameplayMessage("Burritos"));
            await MessageSystem.MessageManager.ProcessMessagesAsync();
            Assert.AreEqual("Burritos", testObject.StringProp);
        }

        [Test]
        [Category("Messaging System Tests")]
        [Order(26)]
        public async Task Message_Send_Gameplay_To_All_Async()
        {
            await MessageSystem.MessageManager.BroadcastAsync(new GameplayMessage("Burritos"));
            await MessageSystem.MessageManager.ProcessMessagesAsync();
            Assert.AreEqual("Burritos", testObject.StringProp);
        }

        [Test]
        [Category("Messaging System Tests")]
        [Order(27)]
        public async Task Message_Send_System_To_All_Async()
        {
            testObject.stringField = "Burritos";
            testObject.intField = 1234;
            await MessageSystem.MessageManager.BroadcastAsync(new SystemMessage(testObject));
            await MessageSystem.MessageManager.ProcessMessagesAsync();
            Assert.AreEqual("Burritos", testObject.stringField);
            Assert.AreEqual(1234, testObject.intField);
        }



    }
}
