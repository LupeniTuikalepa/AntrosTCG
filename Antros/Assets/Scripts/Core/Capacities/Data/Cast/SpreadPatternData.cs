using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Data
{
    [Serializable]
    public class SpreadPatternData : PatternData
    {
        [field: SerializeField, BoxGroup("Specific"), Min(0)]
        public int Distance { get; private set; }
    }
}