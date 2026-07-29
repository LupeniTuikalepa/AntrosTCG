using System;
using System.Collections.Generic;
using ATCG.Capacities;
using UnityEngine;

namespace ATCG
{
    [Serializable]
    public class DefaultCapacityProvider : ICapacityDataProvider
    {
        [field: SerializeField]
        public CapacityData[] Capacities { get; private set; }

        public IEnumerable<CapacityData> GetCapacities() => Capacities;
    }
}