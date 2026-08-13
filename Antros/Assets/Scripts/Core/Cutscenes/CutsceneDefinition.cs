using System;
using System.Collections.Generic;
using ATCG.Databases;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;
using UnityEngine.Timeline;

namespace ATCG.Cutscenes
{
    /// <summary>
    /// Abstract, editable definition of a cutscene: the director prefab (which owns the authored
    /// timeline) plus the steps the cutscene can fire at its markers. This is the shared data anchor
    /// for every cutscene-driven system — capacities, physical attacks, passive/event activations,
    /// card arrivals — so they all reuse the same runtime player and the same editor authoring.
    ///
    /// Both members are concrete here so subclasses stay boilerplate-free: they just serialize the
    /// director on the base field and override <see cref="DeclaredSteps"/> when they have any.
    /// </summary>
    public abstract class CutsceneDefinition : GameDatabaseObject
    {
        // The cutscene stage as a prefab; its PlayableDirector already owns the authored TimelineAsset
        // (via playableAsset), so every kind references the director rather than a second, redundant
        // timeline reference. FormerlySerializedAs migrates capacity assets that stored this under the
        // legacy CutsceneDirector field, so no per-asset migration is needed.
        [field: SerializeField]
        [field: FormerlySerializedAs("<CutsceneDirector>k__BackingField")]
        public PlayableDirector Director { get; private set; }

        /// <summary>Convenience: the timeline the director plays, if any.</summary>
        public TimelineAsset Timeline => Director != null ? Director.playableAsset as TimelineAsset : null;

        /// <summary>
        /// The step names this cutscene can fire at its markers. Empty by default; kinds that have
        /// steps override it (attacks with a fixed set, capacities from their source-gen struct).
        /// </summary>
        public virtual IReadOnlyList<string> DeclaredSteps => Array.Empty<string>();
    }
}
