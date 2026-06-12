using System;
using UnityEngine;

namespace ATCG.Capacities.Data
{
    [Serializable]
    public struct FloodFillHexCapacityPatternData : IHexCapacityPatternData
    {
        [field: SerializeField]
        public int Distance { get; private set; }
    }
}