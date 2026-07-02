using System.Collections.Generic;

namespace CollectionDebugger.Core
{
    public static partial class CollectionDebug
    {
        private static readonly List<SnapshotWatch> SnapshotWatches;

        public static void TakeSnapshot(string label)
        {
            if (!Watches.TryGetValue(label, out var watch)) 
                return;
            
            var watcher = new SnapshotWatch(label, watch.GetEntries());
            
            SnapshotWatches.Add(watcher);
        }
        
        public static void ClearSnapshots()
        {
            SnapshotWatches.Clear();
        }
        
        internal static List<SnapshotWatch> GetSnapshot() => SnapshotWatches;
    }
}