using System;
using ATCG.Capacities.Data;
using ATCG.Databases;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities
{

    [CreateAssetMenu(menuName =  "ATCG/Capacities/Capacity")]
    public class CapacityData : GameDatabaseObject
    {
        [field: SerializeField, PropertyRange(0, 10), BoxGroup("Base")]
        public int Cost { get; private set; }

        [field: SerializeField, BoxGroup("Base")]
        public string Name { get; private set;}

        [field: SerializeField, TextArea, BoxGroup("Base")]
        public string Description { get; private set; }

        [field: BoxGroup("Behaviour/Patterns")]
        [field: SerializeReference, Tooltip("Patterns of cells that can be selected by the player."), InlineProperty, ListDrawerSettings(ShowFoldout = false)]
        public ICapacityCastPatternData[] CastPatterns { get; private set; }

        [field: BoxGroup("Behaviour/Patterns")]
        [field: SerializeReference, Tooltip("Patterns of cells affected by the capacity"), InlineProperty, ListDrawerSettings(ShowFoldout = false)]
        public ICapacityCastPatternData[] FirePatterns { get; private set; }

        [field: BoxGroup("Behaviours/Effects")]
        [field: SerializeReference, Tooltip("Effect applied on the caster."), InlineProperty, ListDrawerSettings(ShowFoldout = false)]
        public IEffectData[] CasterEffects { get; private set; }

        [field: BoxGroup("Behaviour/Effects"), PropertySpace]
        [field: SerializeReference, Tooltip("Effects applied on cells hit by the capacity"), InlineProperty, ListDrawerSettings(ShowFoldout = false)]
        public IEffectData[] CellsHitEffects { get; private set; }

        [field: BoxGroup("Behaviour/Effects")]
        [field: SerializeReference, Tooltip("Effects applied on actors hit by the capacity"), InlineProperty, ListDrawerSettings(ShowFoldout = false)]
        public IEffectData[] AlliesHitEffects { get; private set; }

        [field: BoxGroup("Behaviour/Effects")]
        [field: SerializeReference, Tooltip("Effects applied on actors hit by the capacity"), InlineProperty, ListDrawerSettings(ShowFoldout = false)]
        public IEffectData[] OpponentsHitEffects { get; private set; }

        protected override void Reset()
        {
            base.Reset();
            CastPatterns = Array.Empty<ICapacityCastPatternData>();
            FirePatterns = Array.Empty<ICapacityCastPatternData>();
            CasterEffects = Array.Empty<IEffectData>();

            CellsHitEffects = Array.Empty<IEffectData>();
            AlliesHitEffects = Array.Empty<IEffectData>();
            OpponentsHitEffects = Array.Empty<IEffectData>();
        }
    }
}