using System.Collections.Generic;

namespace CollectionDebugger.Core
{
    /// <summary>
    /// Main entry point of the CollectionDebugger.
    /// Allows registering collections to monitor and display them
    /// in the Unity editor window under "Tools/Collection Debugger".
    /// </summary>
    public static partial class CollectionDebug
    {
        private static readonly Dictionary<string, ICollectionWatch> Watches;

        static CollectionDebug()
        {
            Watches = new();
            SnapshotWatches = new();
        }

        /// <summary>
        /// Registers an indexed collection (Array, List) in the debugger.
        /// Entries will be displayed with their index as key.
        /// </summary>
        /// <param name="collection">The collection to watch.</param>
        /// <param name="label">Name displayed in the debugger window.</param>
        /// <typeparam name="T">Type of the collection elements.</typeparam>
        /// <example>
        /// <code>
        /// enemies.Watch(nameof(enemies));
        /// </code>
        /// </example>
        /// <example>
        /// <code>
        /// CollectionDebug.Watch(nameof(enemies), enemies);
        /// </code>
        /// </example>
        public static void Watch<T>(this IEnumerable<T> collection, string label)
            => Watches[label] = new IndexedWatch<T>(label, collection);

        /// <summary>
        /// Registers a dictionary in the debugger.
        /// Entries will be displayed with their actual key.
        /// </summary>
        /// <param name="collection">The dictionary to watch.</param>
        /// <param name="label">Name displayed in the debugger window.</param>
        /// <typeparam name="TKey">Type of the dictionary keys.</typeparam>
        /// <typeparam name="TValue">Type of the dictionary values.</typeparam>
        /// <example>
        /// <code>
        /// scores.Watch(nameof(scores));
        /// </code>
        /// </example>
        /// <example>
        /// <code>
        /// CollectionDebug.Watch(nameof(scores), scores);
        /// </code>
        /// </example>
        public static void Watch<TKey, TValue>(this IDictionary<TKey, TValue> collection, string label)
            => Watches[label] = new DictionaryWatch<TKey, TValue>(label, collection);

        /// <summary>
        /// Registers a custom watcher in the debugger.
        /// Use this overload by inheriting from <see cref="CollectionWatchBase"/>
        /// to define your own display logic.
        /// </summary>
        /// <param name="watch">The custom watcher instance to register.</param>
        /// <example>
        /// <code>
        /// var myCustomWatch = new MyCustomWatch()
        /// myCustomWatch.Watch();
        /// </code>
        /// </example>
        /// <example>
        /// <code>
        /// CollectionDebug.Watch(new MyCustomWatch());
        /// </code>
        /// </example>
        public static void Watch(this CollectionWatchBase watch)
            => Watches[watch.Label] = watch;

        /// <summary>
        /// Removes a collection from the debugger.
        /// Call this in your MonoBehaviour's OnDisable() or OnDestroy().
        /// </summary>
        /// <param name="label">Label of the collection to remove.</param>
        public static void Unwatch(string label)
            => Watches.Remove(label);

        /// <summary>
        /// Removes all registered collections from the debugger.
        /// </summary>
        public static void UnwatchAll()
            => Watches.Clear();

        internal static IReadOnlyDictionary<string, ICollectionWatch> GetWatches() => Watches;
    }
}