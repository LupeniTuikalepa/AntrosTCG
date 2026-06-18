using System.Collections.Generic;

namespace CollectionDebugger.Core
{
    // Watcher implementation for IDictionary<TKey, TValue>.
    // Unlike IndexedWatch, entries are displayed with their actual dictionary key
    // instead of a numeric index.
    internal class DictionaryWatch<TKey, TValue> : CollectionWatchBase
    {
        // IDictionary is a reference type — modifications to the original dictionary
        // are automatically reflected on the next repaint without re-registering.
        private readonly IDictionary<TKey, TValue> collection;

        public DictionaryWatch(string label, IDictionary<TKey, TValue> collection) : base(label)
        {
            this.collection = collection;
        }

        // IDictionary always exposes Count — no fallback needed, always O(1).
        protected override int GetCount() => collection.Count;

        // Fills the entries array with the current key-value pairs of the dictionary.
        // The index variable is used to track the position in the entries array,
        // it is not exposed as the entry key — the actual dictionary key is used instead.
        protected override void FillEntries(WatchEntry[] entries)
        {
            int index = 0;
            foreach (var (key, value) in collection)
            {
                entries[index] = new WatchEntry(key, value);
                index++;
            }
        }
    }
}