using System;
using System.Collections.Generic;
using ATCG.Databases;
using ATCG.HexGrids.Patterns.Building;
using ATCG.Capacities.Properties;
using ATCG.Elements;
using ATCG.Enums;
using Helteix.Tools.DataMapping;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ATCG.Capacities
{
    public abstract class CapacityData : GameDatabaseObject, IData, IAbility
    {
        [field: SerializeField, PropertyRange(0, 10), BoxGroup("Base")]
        public int Cost { get; private set; }

        [field: SerializeField, BoxGroup("Base")]
        public string Name { get; private set; }

        [field: SerializeField, BoxGroup("Base")]
        public Element Element { get; private set; }

        [field: SerializeField, TextArea, BoxGroup("Base")]
        public string Description { get; private set; }

        [field: SerializeField, TextArea, BoxGroup("Base")]
        public Sprite Icon { get; private set; }

        [field: BoxGroup("Base")]
        [field: SerializeField, Tooltip("Patterns of cells that can be selected by the player."), InlineProperty, ListDrawerSettings(ShowFoldout = false)]
        public PatternGroup CastPatterns { get; private set; }

        // The cutscene stage as a prefab. Its PlayableDirector already owns the
        // authored TimelineAsset (via playableAsset), so we reference the director
        // directly rather than storing a second, redundant timeline reference.
        [field: BoxGroup("Base")]
        [field: SerializeField, BoxGroup("Base")]
        public PlayableDirector CutsceneDirector { get; private set; }

        // Declared, tweakable capacity properties. [SerializeReference] + Odin's
        // dropdown lets each entry be any ICapacityPropertyDefinition implementation
        // (one class per type, in Battle). The context pre-fills its closed schema from
        // these; only declared properties can be written at runtime.
        [field: SerializeReference, BoxGroup("Properties")]
        public List<ICapacityPropertyDefinition> PropertyDefinitions { get; private set; } = new();

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
    }
}