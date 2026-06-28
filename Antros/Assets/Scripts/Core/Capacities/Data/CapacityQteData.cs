using System;
using UnityEngine;

namespace ATCG.Capacities
{
    [Serializable]
    public struct CapacityQteData
    {
        [field: SerializeField]
        public float Duration { get; private set; }
    }
}