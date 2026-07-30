using System;
using System.Collections.Generic;
using UnityEngine;

namespace ATCG.Debugging.Cheats
{
    /// <summary>
    /// Services handed to a cheat when it runs. The editor Cheats tool wires <see cref="Picker"/>
    /// so a cheat can ask the user to choose a target by name through an editor popup (grid/world
    /// selection still goes through the in-game phases). Cheats that need no input ignore it.
    /// </summary>
    public sealed class CheatContext
    {
        /// <summary>
        /// Set by the editor Cheats tool: shows a picker for the given options and resolves to the
        /// chosen one (empty string if cancelled).
        /// </summary>
        public Func<string, IReadOnlyList<string>, Awaitable<string>> Picker;

        /// <summary>
        /// Asks the user to pick one option; resolves to the chosen string, or empty if cancelled
        /// or if no picker is wired (e.g. invoked outside the editor tool).
        /// </summary>
        public Awaitable<string> Choose(string title, IReadOnlyList<string> options)
        {
            if (Picker != null)
                return Picker(title, options);

            AwaitableCompletionSource<string> source = new();
            source.SetResult(string.Empty);
            return source.Awaitable;
        }
    }
}
