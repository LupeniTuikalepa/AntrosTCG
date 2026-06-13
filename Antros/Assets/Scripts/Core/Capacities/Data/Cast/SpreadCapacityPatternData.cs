using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Data
{
    [Serializable]
    public class SpreadCapacityPatternData : CapacityPatternData
    {
        [field: SerializeField, BoxGroup("Specific"), Min(0)]
        public int Distance { get; private set; }
    }
}