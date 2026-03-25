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

        [field: BoxGroup("Behaviour")]
        [field: SerializeReference, Tooltip("Patterns of cells that can be selected by the player."), InlineProperty, ListDrawerSettings(ShowFoldout = false)]
        public ICapacityCastPatternData[] CastPatterns { get; private set; }

        [field: BoxGroup("Behaviour")]
        [field: SerializeReference, Tooltip("Patterns of cells affected by the capacity"), InlineProperty, ListDrawerSettings(ShowFoldout = false)]
        public ICapacityCastPatternData[] FirePatterns { get; private set; }

        [field: BoxGroup("Behaviour")]
        [field: SerializeReference, Tooltip("Effect applied on the caster."), InlineProperty, ListDrawerSettings(ShowFoldout = false)]
        public ICapacityHitEffectData[] CasterEffects { get; private set; }

        [field: BoxGroup("Behaviour")]
        [field: SerializeReference, Tooltip("Effects applied on cells and actors hit by the capacity"), InlineProperty, ListDrawerSettings(ShowFoldout = false)]
        public ICapacityHitEffectData[] HitEffects { get; private set; }

        protected override void Reset()
        {
            base.Reset();
            CastPatterns  = Array.Empty<ICapacityCastPatternData>();
            FirePatterns = Array.Empty<ICapacityCastPatternData>();
            CasterEffects = Array.Empty<ICapacityHitEffectData>();
            HitEffects = Array.Empty<ICapacityHitEffectData>();
        }
    }
}