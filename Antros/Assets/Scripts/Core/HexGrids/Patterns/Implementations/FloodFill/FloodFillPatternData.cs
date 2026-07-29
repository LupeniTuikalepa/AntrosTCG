using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.HexGrids.Patterns
{
    [Serializable]
    public class FloodFillPatternData : PatternData
    {
        [field: SerializeField, BoxGroup("Specific"), Min(0)]
        public int Distance { get; private set; }
        public FloodFillPatternData(int distance)
        {
            Distance = distance;
        }

    }
}