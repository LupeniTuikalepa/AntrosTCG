using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.HexGrids.Patterns
{
    [Serializable]
    public class SpiralPatternData : PatternData
    {
        [field: SerializeField, BoxGroup("Specific")]
        public int Distance { get; private set; }
    }
}