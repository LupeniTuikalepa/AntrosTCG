using System;
using ATCG.HexGrids;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Data
{
    [Serializable]
    public class RayPatternData : CapacityPatternData
    {
        [field: SerializeField, BoxGroup("Specific")]
        public HexCoordinates Direction { get; private set; }
    }
}