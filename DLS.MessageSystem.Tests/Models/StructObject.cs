namespace DLS.MessageSystem.Tests.Models
{
    public struct StructProperty
    {
        public int structIntField = 77;
        public string structStringField = "Hello structStringField";
        public NestedClass structNestedClassField = new();
        public int StructIntValueProp { get; set; }
        public string StructStringValueProp { get; set; }
        public NestedClass StructNestedClassProp { get; set; }
        
        
        public StructProperty()
        {
            StructIntValueProp = 420;
            StructStringValueProp = "Hello StructStringValue";
            StructNestedClassProp = new();
        }
    }
}