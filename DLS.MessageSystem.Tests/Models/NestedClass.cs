namespace DLS.MessageSystem.Tests.Models
{
    public class NestedClass
    {
        public int nestedIntField = 50;
        public string nestedStringField = "Hello nestedStringField";
        public DateTime nestedDateTimeField = DateTime.Today.Date;
        public InnerNestedClass innerNestedClassField = new();

        public List<int> nestedIntListField = new()
        {
            { 1 },
            { 2 },
            { 3 },
            { 4 },
            { 5 }
        };
        
        public Dictionary<int, string> nestedStringDictionaryField = new ()
        {
            { 1, "nestedStringDictionaryFieldValue 1" },
            { 2, "nestedStringDictionaryFieldValue 2" },
            { 3, "nestedStringDictionaryFieldValue 3" },
            { 4, "nestedStringDictionaryFieldValue 4" },
            { 5, "nestedStringDictionaryFieldValue 5" }
        };
        
        public int NestedIntProp { get; set; }
        public string NestedStringProp { get; set; }
        public List<string> NestedStringListProp { get; set; }
        
        public Dictionary<int,string> NestedStringDictionaryProp { get; set; }
        public InnerNestedClass InnerNestedClassProp { get; set; }

        public NestedClass()
        {
            NestedIntProp = 99;
            NestedStringProp = "Hello nestedStringProp";
            NestedStringListProp = new ()
            {
                { "Hello NestedStringListProp 1" },
                { "Hello NestedStringListProp 2" },
                { "Hello NestedStringListProp 3" },
                { "Hello NestedStringListProp 4" },
                { "Hello NestedStringListProp 5" },
            };
            NestedStringDictionaryProp = new()
            {
                { 1, "nestedStringDictionaryPropValue 1" },
                { 2, "nestedStringDictionaryPropValue 2" },
                { 3, "nestedStringDictionaryPropValue 3" },
                { 4, "nestedStringDictionaryPropValue 4" },
                { 5, "nestedStringDictionaryPropValue 5" }
            };
            InnerNestedClassProp = new InnerNestedClass();
        }
    
        public string NestedStringReturnMethod()
        {
            return "NestedStringReturnMethod";
        }
        public string NestedStringReturnMethod(string val)
        {
            return "NestedStringReturnMethod" + val;
        }
        public string NestedStringReturnMethod(string val, string val2)
        {
            return "NestedStringReturnMethod" + val + val2;
        }

        public int NestedIntReturnMethod()
        {
            return 420;
        }
        
        public int NestedIntReturnMethod(int val)
        {
            return val;
        }
        
        public int NestedIntReturnMethod(int val, int val2)
        {
            return val + val2;
        }

        public string NestedMultipleTypeParameterReturnStringMethod(string val, int val2, bool isTrue)
        {
            var returnValue = string.Empty;
            returnValue += $"{val} {val2} {isTrue}";
            return returnValue;
        }
    }
}