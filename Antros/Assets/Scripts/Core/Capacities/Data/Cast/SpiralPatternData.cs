using System;
using ATCG.HexGrids;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Data
{
    [Serializable]
    public class SpiralPatternData : CapacityPatternData
    {
        [field: SerializeField, BoxGroup("Specific")]
        public int Distance { get; private set; }
    }
}