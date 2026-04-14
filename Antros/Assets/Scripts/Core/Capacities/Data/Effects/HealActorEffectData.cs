using System;
using ATCG.Battle.Cards.Capacities.Interfaces;
using UnityEngine;

namespace ATCG.Capacities.Data.Effects
{
    [Serializable]
    public class HealEffectData : IEffectData
    {
        [field: SerializeField, Min(0)]
        public int Quantity { get; private set; }
    }
}