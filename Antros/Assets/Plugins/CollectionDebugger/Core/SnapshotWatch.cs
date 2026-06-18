namespace CollectionDebugger.Core
{
    internal class SnapshotWatch : CollectionWatchBase
    {
        private readonly WatchEntry[] snapshot;

        public SnapshotWatch(string label, WatchEntry[] snapshot) : base(label)
            => this.snapshot = snapshot;

        protected override int GetCount() => snapshot.Length;
        protected override void FillEntries(WatchEntry[] entries)
            => System.Array.Copy(snapshot, entries, snapshot.Length);
    }
}