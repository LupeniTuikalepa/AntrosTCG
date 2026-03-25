using System;
using UnityEngine;

namespace ATCG.Capacities.Data
{
    [Serializable]
    public class FloodFillPatternData : ICapacityCastPatternData
    {
        [field: SerializeField]
        public int Distance { get; private set; }
    }
}