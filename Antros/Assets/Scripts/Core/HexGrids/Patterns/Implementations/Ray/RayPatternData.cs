using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.HexGrids.Patterns
{
    [Serializable]
    public class RayPatternData : PatternData
    {
        [field: SerializeField, BoxGroup("Specific")]
        public HexDirection Direction { get; private set; }
        [field: SerializeField, BoxGroup("Specific")]
        public int Range { get; private set; }
    }
}