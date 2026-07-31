using System.Collections.Generic;

namespace ATCG.Debugging.Cheats
{
    /// <summary>
    /// A named bundle of cheats contributed by a <see cref="CheatProvider"/>. Rendered as a
    /// top-level group in the editor Cheats tool (e.g. "Player 1", "Player 2", "System").
    /// </summary>
    public sealed class CheatSection
    {
        public string Name { get; }
        public IEnumerable<ICheat> Cheats { get; }

        /// <summary>
        /// Whether this section's cheats can actually run in the current context. When false the
        /// tool still shows them (for discoverability) but greyed out and non-interactive — e.g. a
        /// preview built with no live player behind it.
        /// </summary>
        public bool Enabled { get; }

        public CheatSection(string name, IEnumerable<ICheat> cheats, bool enabled = true)
        {
            Name = name;
            Cheats = cheats;
            Enabled = enabled;
        }
    }
}
