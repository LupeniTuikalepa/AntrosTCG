using System.Collections.Generic;
using UnityEngine;

namespace ATCG.Debugging.Cheats
{
    /// <summary>
    /// A source of cheats, discovered by reflection — NOT a scene component, so nothing pollutes
    /// the prefabs/build. A provider inspects the live runtime itself (e.g. the active battle) to
    /// decide whether it can contribute (<see cref="IsAvailable"/>) and what it exposes
    /// (<see cref="GetSections"/>). Each <see cref="CheatSection"/> becomes a top-level group in
    /// the editor Cheats tool; within a section, cheats are further grouped by their
    /// <see cref="CheatGroupAttribute"/>. Subclasses need a public parameterless constructor.
    /// </summary>
    public abstract class CheatProvider
    {
        /// <summary>Cheap gate: can this provider contribute right now? Defaults to "only in play mode".</summary>
        public virtual bool IsAvailable => Application.isPlaying;

        /// <summary>The sections this provider contributes given the current runtime state.</summary>
        public abstract IEnumerable<CheatSection> GetSections();
    }
}
