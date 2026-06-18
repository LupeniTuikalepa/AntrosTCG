namespace CollectionDebugger.Core
{
    /// <summary>
    /// Represents a single entry in a watched collection.
    /// Returned by <see cref="ICollectionWatch.GetEntries"/> and used by the debugger window for display.
    /// </summary>
    public readonly struct WatchEntry
    {
        /// <summary>
        /// The key of this entry.
        /// For indexed collections (Array, List), this is the index.
        /// For dictionaries, this is the actual key.
        /// </summary>
        public readonly object key;

        /// <summary>
        /// The value of this entry.
        /// </summary>
        public readonly object value;

        /// <param name="key">Index or dictionary key.</param>
        /// <param name="value">Value of the entry.</param>
        public WatchEntry(object key, object value)
        {
            this.key = key;
            this.value = value;
        }
    }
}