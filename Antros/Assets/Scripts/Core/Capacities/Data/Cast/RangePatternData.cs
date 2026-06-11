using System;
using UnityEngine;

namespace ATCG.Capacities.Data
{
    [Serializable]
    public class SpreadPatternData : ICapacityPatternData
    {
        [field: SerializeField]
        public int Distance { get; private set; }
    }
}