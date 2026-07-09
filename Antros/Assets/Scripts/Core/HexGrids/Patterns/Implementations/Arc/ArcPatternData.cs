using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.HexGrids.Patterns.Arc
{
    [Serializable]
    public class ArcPatternData : PatternData
    {
        [field: SerializeField, BoxGroup("Specific")]
        public HexCoordinates Center { get; private set; }
        
        [field: SerializeField, BoxGroup("Specific")]
        public HexCoordinates To { get; private set; }
        
        [field: SerializeField, BoxGroup("Specific")]
        public int Radius { get; private set; }
    }
}