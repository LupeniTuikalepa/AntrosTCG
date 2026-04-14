using System;
using ATCG.Battle.Cards.Capacities.Interfaces;
using UnityEngine;

namespace ATCG.Capacities.Data.Effects
{
    [Serializable]
    public class DamageEffectData : IEffectData
    {
        [field: SerializeField, Min(0)]
        public int Quantity { get; private set; }
    }
}