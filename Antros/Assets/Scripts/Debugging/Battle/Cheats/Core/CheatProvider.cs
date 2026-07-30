using System.Collections.Generic;
using UnityEngine;

namespace ATCG.Debugging.Cheats
{
    /// <summary>
    /// Scene-side source of cheats. Drop a subclass on a GameObject; the editor's Cheats tool
    /// asks every CheatProvider present in the loaded scene(s) for its cheats, so the tool never
    /// needs to know which scene it is in — it just queries "who has cheats?" and shows them.
    /// In the UI, cheats are grouped by their provider (see <see cref="DisplayName"/>) and then
    /// by their <see cref="CheatGroupAttribute"/>.
    /// </summary>
    public abstract class CheatProvider : MonoBehaviour
    {
        /// <summary>Label used as the provider's group header. Defaults to the GameObject name.</summary>
        public virtual string DisplayName => name;

        /// <summary>The cheats this provider supplies (rebuilt on demand, so live refs stay fresh).</summary>
        public abstract IEnumerable<ICheat> GetCheats();
    }
}
