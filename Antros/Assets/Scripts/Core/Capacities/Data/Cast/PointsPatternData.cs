using System;
using ATCG.HexGrids;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Data
{
    [Serializable]
    public class PointsPatternData : CapacityPatternData
    {
        [field: SerializeField, BoxGroup("Specific")]
        public HexCoordinates[] Points { get; private set; }
    }
}