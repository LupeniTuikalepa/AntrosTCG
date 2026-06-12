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
        [field: BoxGroup("Behaviour/Patterns")]
        [field: SerializeReference, Tooltip("Patterns of cells that can be selected by the player."), InlineProperty, ListDrawerSettings(ShowFoldout = false)]
        public IHexCapacityPatternData[] CastPatterns { get; private set; }

        [field: BoxGroup("Behaviour/Patterns")]
        [field: SerializeReference, Tooltip("Patterns of cells affected by the capacity"), InlineProperty, ListDrawerSettings(ShowFoldout = false)]
        public IHexCapacityPatternData[] FirePatterns { get; private set; }

        [field: BoxGroup("Behaviour/Effects")]
        [field: SerializeReference, Tooltip("Effect applied on the caster."), InlineProperty, ListDrawerSettings(ShowFoldout = false)]
        public IEffectData[] CasterEffects { get; private set; }

        [field: BoxGroup("Behaviour/Effects"), PropertySpace]
        [field: SerializeReference, Tooltip("Effects applied on everything hit by the capacity"), InlineProperty, ListDrawerSettings(ShowFoldout = false)]
        public IEffectData[] HitEffects { get; private set; }

        protected override void Reset()
        {
            base.Reset();
            CastPatterns = Array.Empty<IHexCapacityPatternData>();
            FirePatterns = Array.Empty<IHexCapacityPatternData>();
            CasterEffects = Array.Empty<IEffectData>();
        }
    }
}