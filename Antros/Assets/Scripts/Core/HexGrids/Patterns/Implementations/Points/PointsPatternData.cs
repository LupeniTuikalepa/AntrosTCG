using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.HexGrids.Patterns
{
    [Serializable]
    public class PointsPatternData : PatternData
    {
        [field: SerializeField, BoxGroup("Specific")]
        public HexCoordinates[] Points { get; private set; }
    }
}