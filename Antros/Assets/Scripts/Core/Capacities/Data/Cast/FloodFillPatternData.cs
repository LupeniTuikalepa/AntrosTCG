using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Data
{
    [Serializable]
    public class FloodFillPatternData : CapacityPatternData
    {
        [field: SerializeField, BoxGroup("Specific"), Min(0)]
        public int Distance { get; private set; }
    }
}