using System;
using ATCG.HexGrids;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Data
{
    [Serializable]
    public class OffsetsPatternData : CapacityPatternData
    {
        [field: SerializeField, BoxGroup("Specific")]
        public HexCoordinates[] Offsets { get; private set; }
    }
}