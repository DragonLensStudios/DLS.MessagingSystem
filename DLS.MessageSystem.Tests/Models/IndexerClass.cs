namespace DLS.MessageSystem.Tests.Models
{
    public class IndexerClass
    {
        private Dictionary<int, string> indexerIntDictionary = new Dictionary<int, string>();
        private Dictionary<string, string> indexerStringDictionary = new Dictionary<string, string>();

        public IndexerClass()
        {
            indexerIntDictionary = new Dictionary<int, string>
            {
                {1, "IndexerClassData Value 1"},
                {2, "IndexerClassData Value 2"},
                {3, "IndexerClassData Value 3"},
                {4, "IndexerClassData Value 4"},
                {5, "IndexerClassData Value 5"},
                {6, "IndexerClassData Value 6"},
                {7, "IndexerClassData Value 7"},
                {8, "IndexerClassData Value 8"},
                {9, "IndexerClassData Value 9"},
                {10, "IndexerClassData Value 10"},
            };
            indexerStringDictionary = new Dictionary<string, string>
            {
                { "Test 1", "IndexerClassData Value 1" },
                { "Test 2", "IndexerClassData Value 2" },
                { "Test 3", "IndexerClassData Value 3" },
                { "Test 4", "IndexerClassData Value 4" },
                { "Test 5", "IndexerClassData Value 5" },
                { "Test 6", "IndexerClassData Value 6" },
                { "Test 7", "IndexerClassData Value 7" },
                { "Test 8", "IndexerClassData Value 8" },
                { "Test 9", "IndexerClassData Value 9" },
                { "Test 10", "IndexerClassData Value 10" },
            };
        }
    
        public string this[int index]
        {
            get { return indexerIntDictionary.ContainsKey(index) ? indexerIntDictionary[index] : null; }
            set { indexerIntDictionary[index] = value; }
        }

        public string this[string key]
        {
            get { return indexerStringDictionary.ContainsKey(key) ? indexerStringDictionary[key] : null; }
            set {indexerStringDictionary[key] = value; }
        }
        public string GetIndexerValue(int index)
        {
            return indexerIntDictionary.ContainsKey(index) ? indexerIntDictionary[index] : null;
        }
    
        public void SetIndexerValue(int index, string value)
        {
            indexerIntDictionary[index] = value;
        }
    }
}