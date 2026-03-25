using System;
using ATCG.Battle.Cards.Capacities.Interfaces;
using UnityEngine;

namespace ATCG.Capacities.Data.Effects
{
    [Serializable]
    public class DealDamageEffectData : ICapacityHitEffectData
    {
        [field: SerializeField]
        public int Quantity { get; private set; }
        [field: SerializeField]
        public CapacityTargetType TargetType { get; private set; }
    }
}