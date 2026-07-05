using System;
using System.Collections.Generic;
using ATCG.Databases;
using ATCG.Enums;
using ATCG.HexGrids.Patterns.Building;
using Helteix.Tools.DataMapping;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ATCG.Capacities
{
    public abstract class CapacityData : GameDatabaseObject, IData
    {
        [field: SerializeField, PropertyRange(0, 10), BoxGroup("Base")]
        public int Cost { get; private set; }

        [field: SerializeField, BoxGroup("Base")]
        public string Name { get; private set; }

        [field: SerializeField, TextArea, BoxGroup("Base")]
        public string Description { get; private set; }
        [field: SerializeField, BoxGroup("Base")]
        public Element Element { get; private set; }

        [field: BoxGroup("Base")]
        [field: SerializeField, Tooltip("Patterns of cells that can be selected by the player."), InlineProperty, ListDrawerSettings(ShowFoldout = false)]
        public PatternGroup CastPatterns { get; private set; }

        // The cutscene stage as a prefab. Its PlayableDirector already owns the
        // authored TimelineAsset (via playableAsset), so we reference the director
        // directly rather than storing a second, redundant timeline reference.
        [field: BoxGroup("Base")]
        [field: SerializeField, BoxGroup("Base")]
        public PlayableDirector CutsceneDirector { get; private set; }

        // Convenience accessor: the timeline the director plays, if any.
        public TimelineAsset CutsceneTimeline => CutsceneDirector != null
            ? CutsceneDirector.playableAsset as TimelineAsset
            : null;

        private Dictionary<string, CapacityStepData> mappedSteps;

        protected virtual void OnEnable() => RebuildStepMap();

        protected override void OnValidate()
        {
            base.OnValidate();
            RebuildStepMap();
        }

        private void RebuildStepMap()
        {
            mappedSteps ??= new Dictionary<string, CapacityStepData>();
            mappedSteps.Clear();
            MapSteps(mappedSteps);
        }

        protected abstract void MapSteps(Dictionary<string, CapacityStepData> capacityStepDatas);

        public bool TryGetStep(string stepName, out CapacityStepData step)
        {
            mappedSteps ??= new Dictionary<string, CapacityStepData>();
            return mappedSteps.TryGetValue(stepName, out step);
        }

        /// <summary>
        /// Resolves a declared step or throws. Generated accessors
        /// (XxxStepData) route through here, so a step declared via
        /// [WithStep] but missing from the asset fails loudly instead of
        /// silently returning a default CapacityStepData.
        /// </summary>
        protected CapacityStepData GetStepOrThrow(string stepName)
        {
            if (TryGetStep(stepName, out CapacityStepData step))
                return step;

            throw new InvalidOperationException(
                $"[{name}] Step '{stepName}' is declared on '{GetType().Name}' but " +
                $"absent from the asset. Resync the steps array.");
        }
    }
}