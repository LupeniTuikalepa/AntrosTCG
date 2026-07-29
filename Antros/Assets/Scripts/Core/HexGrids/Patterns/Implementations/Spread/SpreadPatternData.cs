using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.HexGrids.Patterns
{
    [Serializable]
    public class SpreadPatternData : PatternData
    {
        [field: SerializeField, BoxGroup("Specific"), Min(0)]
        public int Distance { get; private set; }

        public SpreadPatternData(int distance)
        {
            Distance = distance;
        }
    }
}