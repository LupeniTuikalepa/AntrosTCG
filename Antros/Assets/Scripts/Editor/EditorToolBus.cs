using System;
using System.Collections.Generic;

namespace ATCG.Editor
{
    /// <summary>
    /// A tiny typed publish/subscribe bus shared by all hub tools. It lets tools talk
    /// without referencing each other: the World Inspector publishes "entity selected",
    /// the Command Timeline (or the grid view) listens and reacts.
    ///
    /// Editor-only, single-threaded (Unity main thread), so no locking. Handlers are
    /// keyed by event type. Tools should subscribe in OnActivated and unsubscribe in
    /// OnDeactivated to avoid stale handlers across tool switches.
    /// </summary>
    public static class EditorToolBus
    {
        private static readonly Dictionary<Type, Delegate> handlers = new();

        public static void Subscribe<T>(Action<T> handler)
        {
            if (handler == null) return;
            handlers.TryGetValue(typeof(T), out Delegate existing);
            handlers[typeof(T)] = (existing as Action<T>) + handler;
        }

        public static void Unsubscribe<T>(Action<T> handler)
        {
            if (handler == null) return;
            if (!handlers.TryGetValue(typeof(T), out Delegate existing))
                return;
            Action<T> updated = (existing as Action<T>) - handler;
            if (updated == null)
                handlers.Remove(typeof(T));
            else
                handlers[typeof(T)] = updated;
        }

        public static void Publish<T>(T evt)
        {
            if (handlers.TryGetValue(typeof(T), out Delegate d) && d is Action<T> action)
                action(evt);
        }

        /// <summary>Drop every subscription (e.g. on play-mode change or hub close).</summary>
        public static void Reset() => handlers.Clear();
    }
}
