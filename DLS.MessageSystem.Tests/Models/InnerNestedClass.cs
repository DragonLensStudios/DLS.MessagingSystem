namespace DLS.MessageSystem.Tests.Models
{
    public class InnerNestedClass
    {
        public int innerNestedIntField = 25;
        public string innerNestedStringField = "Hello innerNestedStringField";
        public int InnerNestedIntProp { get; set; }
        public string InnerNestedStringProp { get; set; }

        public InnerNestedClass()
        {
            InnerNestedIntProp = 50;
            InnerNestedStringProp = "InnerNestedStringProp";
        }

        public string InnerNestedMethodReturnsString()
        {
            return "InnerNestedTestMethodWithString";
        }
    }
}