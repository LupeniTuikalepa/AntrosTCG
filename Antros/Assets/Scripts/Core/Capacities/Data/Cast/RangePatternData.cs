using System;
using UnityEngine;

namespace ATCG.Capacities.Data
{
    [Serializable]
    public struct SpreadHexCapacityPatternData : IHexCapacityPatternData
    {
        [field: SerializeField]
        public int Distance { get; private set; }
    }
}