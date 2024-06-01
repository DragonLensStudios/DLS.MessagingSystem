namespace DLS.MessageSystem.Tests.Models
{
    public class TestClass
    {
        public int intField = 900;
        public string stringField = "Hello StringField";
        public bool voidMethodCalled = false;
        public int voidMethodIntValue = 0;
        public string voidMethodStringValue = string.Empty;
        public DateTime dateTimeField = DateTime.Today.Date;
        public NestedClass nestedClassField = new();
        public TestClass testClassField;

        public int[,] multiDimensionalIntArrayField = new int[,]
        {
            { 1, 2, 3 },
            { 4, 5, 6 },
            { 7, 8, 9 }
        };
        
        public int[][] jaggedIntArrayField = new int[][]
        {
            new[] { 1, 2, 3 },
            new[] { 4, 5, 6 },
            new[] { 7, 8, 9 }
        };

        public List<string> stringListField = new()
        {
            { "Hello stringListField 1" },
            { "Hello stringListField 2" },
            { "Hello stringListField 3" },
            { "Hello stringListField 4" },
            { "Hello stringListField 5" }

        };

        public Dictionary<string, string> stringDictionaryField = new ()
        {
            { "stringDictionaryFieldKey 1", "stringDictionaryFieldValue 1" },
            { "stringDictionaryFieldKey 2", "stringDictionaryFieldValue 2" },
            { "stringDictionaryFieldKey 3", "stringDictionaryFieldValue 3" },
            { "stringDictionaryFieldKey 4", "stringDictionaryFieldValue 4" },
            { "stringDictionaryFieldKey 5", "stringDictionaryFieldValue 5" }
        };
        
        public Dictionary<int, string> intDictionaryField = new ()
        {
            { 1, "intDictionaryFieldValue 1" },
            { 2, "intDictionaryFieldValue 2" },
            { 3, "intDictionaryFieldValue 3" },
            { 4, "intDictionaryFieldValue 4" },
            { 5, "intDictionaryFieldValue 5" }
        };
        
        public int IntProp { get; set; }
        public string StringProp { get; set; }
        public DateTime DateTimeProp { get; set; }
        public NestedClass NestedClassProp { get; set; }
        
        public TestClass TestClassProp { get; set; }
        public List<int> IntListProp { get; set; }
        public List<string> StringListProp { get; set; }
        public Dictionary<string, string> StringDictionaryProp { get; set; }
        public StructProperty StructProp { get; set; }
        public IndexerClass IndexerProp { get; set; }
        public IndexerClassWithOneIndexer IndexerOneIndexerProp { get; set; }
        
        public string[,] MultiDimensionalStringArrayProp { get; set; }
        public string[][] JaggedStringArrayProp { get; set; }

        public TestClass()
        {
            IntProp = 42;
            StringProp = "Hello StringProperty";
            DateTimeProp = DateTime.Today.Date;
            NestedClassProp = new NestedClass
            {
                NestedIntProp = 100,
                NestedStringProp = "Hello NestedString",
                InnerNestedClassProp = new InnerNestedClass()
            };
            IntListProp = new()
            {
                {1},
                {2},
                {3},
                {4},
                {5}
            };
            StringListProp = new()
            {
                {"TestString 1"},
                {"TestString 2"},
                {"TestString 3"},
                {"TestString 4"},
                {"TestString 5"},
            };
            StringDictionaryProp = new()
            {
                { "StringDictionaryPropKey 1", "StringDictionaryPropValue 1" },
                { "StringDictionaryPropKey 2", "StringDictionaryPropValue 2" },
                { "StringDictionaryPropKey 3", "StringDictionaryPropValue 3" },
                { "StringDictionaryPropKey 4", "StringDictionaryPropValue 4" },
                { "StringDictionaryPropKey 5", "StringDictionaryPropValue 5" }
            };
            StructProp = new();
            IndexerProp = new IndexerClass();
            IndexerOneIndexerProp = new IndexerClassWithOneIndexer();
            MultiDimensionalStringArrayProp = new string[,]
            {
                { "Test 1", "Test 2", "Test 3" },
                { "Test 4", "Test 5", "Test 6" },
                { "Test 8", "Test 8", "Test 9" },
            };
            JaggedStringArrayProp = new string[][]
            {
                new[] { "Test 1", "Test 2", "Test 3" },
                new[] { "Test 4", "Test 5", "Test 6" },
                new[] { "Test 7", "Test 8", "Test 9" },
            };
        }

        public string this[string key]
        {
            get => stringDictionaryField.ContainsKey(key) ? stringDictionaryField[key] : null;
            set => stringDictionaryField[key] = value;
        }

        public string this[int index]
        {
            get => intDictionaryField[index];
            set => intDictionaryField[index] = value;
        }

        public void VoidReturnMethod()
        {
            voidMethodCalled = true;
        }
        
        public void VoidReturnMethod(int val)
        {
            voidMethodCalled = true;
            voidMethodIntValue = 5;
        }
        
        public void VoidReturnMethod(int val, string stringVal)
        {
            voidMethodCalled = true;
            voidMethodIntValue = 5;
            voidMethodStringValue = stringVal;
        }

        public string StringReturnMethod()
        {
            return "StringReturnMethodString";
        }
        public string StringReturnMethod(string val)
        {
            return "StringReturnMethodString" + val;
        }
        public string StringReturnMethod(string val, string val2)
        {
            return "StringReturnMethodString" + val + val2;
        }

        public int IntReturnMethod()
        {
            return 42;
        }
        
        public int IntReturnMethod(int val)
        {
            return val;
        }
        
        public int IntReturnMethod(int val, int val2)
        {
            return val + val2;
        }
        
        public string MultipleTypeParameterReturnStringMethod(string val, int val2, bool isTrue)
        {
            var returnValue = string.Empty;
            returnValue += $"{val} {val2} {isTrue}";
            return returnValue;
        }
    }
}