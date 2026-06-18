using System.Collections.Generic;
using System.Linq;

namespace CollectionDebugger.Core
{
    // Watcher implementation for indexed collections (Array, List, or any IEnumerable<T>).
    // Unlike DictionaryWatch, entries are displayed with a numeric index as key.
    internal class IndexedWatch<T> : CollectionWatchBase
    {
        // IEnumerable is a reference type — modifications to the original collection
        // are automatically reflected on the next repaint without re-registering.
        private readonly IEnumerable<T> collection;

        public IndexedWatch(string label, IEnumerable<T> collection) : base(label)
        {
            this.collection = collection;
        }

        // Attempts to cast to ICollection<T> first to get Count in O(1).
        // Falls back to LINQ Count() which iterates the entire collection — O(n) with an enumerator allocation.
        // In practice the fallback is rarely hit since Array and List<T> both implement ICollection<T>.
        protected override int GetCount()
            => collection is ICollection<T> col ? col.Count : collection.Count();

        // Fills the entries array with the current elements of the collection.
        // The numeric index is used as the entry key to reflect the position
        // of each element in the sequence.
        protected override void FillEntries(WatchEntry[] entries)
        {
            int index = 0;
            foreach (var item in collection)
            {
                entries[index] = new WatchEntry(index, item);
                index++;
            }
        }
    }
}