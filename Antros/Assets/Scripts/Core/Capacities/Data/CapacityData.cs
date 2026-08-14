using System;
using System.Collections.Generic;
using ATCG.Cutscenes;
using ATCG.Databases;
using ATCG.HexGrids.Patterns.Building;
using ATCG.Capacities.Properties;
using ATCG.Capacities.Setup;
using ATCG.Elements;
using ATCG.Enums;
using Helteix.Tools.DataMapping;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ATCG.Capacities
{
    public abstract class CapacityData : CutsceneDefinition, IData, IAbility
    {
        [field: SerializeField, PropertyRange(0, 10), BoxGroup("Base")]
        public int Cost { get; private set; }

        [field: SerializeField, BoxGroup("Base")]
        public string Name { get; private set; }

        [field: SerializeField, BoxGroup("Base")]
        public Element Element { get; private set; }

        [field: SerializeField, TextArea, BoxGroup("Base")]
        public string Description { get; private set; }

        [field: SerializeField, BoxGroup("Base")]
        public Sprite Icon { get; private set; }

        [field: BoxGroup("Base")]
        [field: SerializeField, Tooltip("Patterns of cells that can be selected by the player."), InlineProperty, ListDrawerSettings(ShowFoldout = false)]
        public PatternGroup CastPatterns { get; private set; }
        
        [field: BoxGroup("Base")]
        [field: SerializeReference]
        public CapacitySetupData[] Setups { get; private set; }
        
        // The cutscene stage's PlayableDirector (and its timeline) now live on the base
        // CutsceneDefinition as Director / Timeline; existing assets that stored it under the legacy
        // CutsceneDirector field are migrated automatically via FormerlySerializedAs on the base.

        // Declared, tweakable capacity properties. [SerializeReference] + Odin's
        // dropdown lets each entry be any ICapacityPropertyDefinition implementation
        // (one class per type, in Battle). The context pre-fills its closed schema from
        // these; only declared properties can be written at runtime.
        [field: SerializeReference, BoxGroup("Properties")]
        public List<ICapacityPropertyDefinition> PropertyDefinitions { get; private set; } = new();

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

        // The steps are source-generated per capacity (via MapSteps), so DeclaredSteps just exposes
        // the mapped keys — keeping the CutsceneDefinition contract in sync with the runtime struct.
        public override IReadOnlyList<string> DeclaredSteps
        {
            get
            {
                if (mappedSteps == null)
                    RebuildStepMap();
                return new List<string>(mappedSteps.Keys);
            }
        }
    }
}