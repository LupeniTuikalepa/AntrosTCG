namespace CollectionDebugger.Core
{
    internal class SnapshotWatch : CollectionWatchBase
    {
        private readonly WatchEntry[] snapshot;
        public readonly string timestamp;

        public SnapshotWatch(string label, WatchEntry[] snapshot) : base(label)
        {
            this.snapshot = snapshot;
            timestamp = System.DateTime.Now.ToString("HH:mm:ss");
        }

        protected override int GetCount() => snapshot.Length;
        protected override void FillEntries(WatchEntry[] entries){}

        public override WatchEntry[] GetEntries() => snapshot;
    }
}