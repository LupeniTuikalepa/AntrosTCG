using System;
using UnityEngine;

namespace ATCG.Capacities
{
    [Serializable]
    public struct CapacityStepData
    {
        [field: SerializeField]
        public string StepName { get; private set; }

        [field: SerializeField, Min(0)]
        public int QTEsCount { get; private set; }
    }
}