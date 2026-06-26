using System.Collections.Generic;

namespace ATCG.Battle.Entities
{
    public partial class World
    {
        // ReSharper disable once InconsistentNaming
        private static readonly List<World> activeWorlds = new();

        /// <summary>
        /// All worlds currently alive. Maintained for editor/debug tooling.
        /// call <see cref="Unregister"/> when a world is torn down (e.g. end of battle) to drop the reference.
        /// </summary>
        public static IReadOnlyList<World> ActiveWorlds => activeWorlds;

        public void Register()
        {
            if (!activeWorlds.Contains(this))
                activeWorlds.Add(this);
        }

        /// <summary>Drop this world from <see cref="ActiveWorlds"/>.</summary>
        public void Unregister()
        {
            activeWorlds.Remove(this);
        }
    }
}