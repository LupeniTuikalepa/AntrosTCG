using System;
using UnityEngine;

namespace ATCG.Capacities
{
    [Serializable]
    public struct CapacityStepData
    {
        [field: SerializeField]
        public string StepName { get; private set; }

        [field: SerializeField]
        public CapacityQteData[] QTEs { get; private set; }
    }
}