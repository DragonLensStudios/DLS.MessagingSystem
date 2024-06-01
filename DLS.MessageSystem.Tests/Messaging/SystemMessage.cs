using DLS.MessageSystem.Tests.Models;

namespace DLS.MessageSystem.Tests.Messaging;

[Serializable]
public struct SystemMessage
{
    public TestClass TestObject { get; }

    public SystemMessage(TestClass testObject)
    {
        TestObject = testObject;
    }
}