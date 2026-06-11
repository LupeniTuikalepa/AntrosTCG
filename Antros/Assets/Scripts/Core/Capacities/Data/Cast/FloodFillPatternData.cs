using System;
using UnityEngine;

namespace ATCG.Capacities.Data
{
    [Serializable]
    public class FloodFillPatternData : ICapacityPatternData
    {
        [field: SerializeField]
        public int Distance { get; private set; }
    }
}