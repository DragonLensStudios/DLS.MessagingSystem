namespace DLS.MessageSystem.Tests.Models;

public class IndexerClassWithOneIndexer
{
        private Dictionary<string, string> indexerStringDictionary = new Dictionary<string, string>();

        public IndexerClassWithOneIndexer()
        {
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
        
        public string this[string key]
        {
            get { return indexerStringDictionary.ContainsKey(key) ? indexerStringDictionary[key] : null; }
            set {indexerStringDictionary[key] = value; }
        }
        
}