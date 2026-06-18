using System.Collections.Generic;

namespace CollectionDebugger.Core
{
    /// <summary>
    /// Contract for all collection watchers registered in the debugger.
    /// Implement this interface directly for full control, or inherit from
    /// <see cref="CollectionWatchBase"/> for a simpler implementation.
    /// </summary>
    public interface ICollectionWatch
    {
        /// <summary>
        /// Name displayed in the debugger window.
        /// </summary>
        string Label { get; }

        /// <summary>
        /// Returns a snapshot of the current state of the watched collection
        /// as an array of <see cref="WatchEntry"/>.
        /// Called by the debugger window on every repaint.
        /// </summary>
        WatchEntry[] GetEntries();
    }
}