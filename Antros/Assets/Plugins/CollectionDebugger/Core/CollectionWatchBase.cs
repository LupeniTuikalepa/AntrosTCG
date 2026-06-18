namespace CollectionDebugger.Core
{
    /// <summary>
    /// Base class for all collection watchers.
    /// Inherit from this class to create a custom watcher with your own display logic.
    /// </summary>
    /// <example>
    /// <code>
    /// public class EnemyWatch : CollectionWatchBase
    /// {
    ///     private readonly EnemyManager manager;
    ///
    ///     public EnemyWatch(EnemyManager manager) : base("Enemies")
    ///         => manager = manager;
    ///
    ///     protected override int GetCount() => manager.AliveCount;
    ///
    ///     protected override void FillEntries(WatchEntry[] entries)
    ///     {
    ///         int index = 0;
    ///         foreach (var enemy in manager.GetAlive())
    ///             entries[index++] = new WatchEntry(index, enemy.Name);
    ///     }
    /// }
    ///
    /// // Registration
    /// CollectionDebug.Watch(new EnemyWatch(enemyManager));
    /// </code>
    /// </example>
    public abstract class CollectionWatchBase : ICollectionWatch
    {
        /// <summary>
        /// Name displayed in the debugger window.
        /// </summary>
        public string Label { get; }

        /// <param name="label">Name displayed in the debugger window.</param>
        protected CollectionWatchBase(string label)
        {
            Label = label;
        }

        /// <summary>
        /// Returns the number of entries in the watched collection.
        /// Used to allocate the <see cref="WatchEntry"/> array before calling <see cref="FillEntries"/>.
        /// </summary>
        protected abstract int GetCount();

        /// <summary>
        /// Fills the pre-allocated entries array with the current state of the watched collection.
        /// </summary>
        /// <param name="entries">Pre-allocated array to fill. Its length matches the value returned by <see cref="GetCount"/>.</param>
        protected abstract void FillEntries(WatchEntry[] entries);

        /// <summary>
        /// Returns a snapshot of the current state of the watched collection as an array of <see cref="WatchEntry"/>.
        /// Called by the debugger window on every repaint.
        /// </summary>
        public WatchEntry[] GetEntries()
        {
            var entries = new WatchEntry[GetCount()];
            FillEntries(entries);
            return entries;
        }
    }
}