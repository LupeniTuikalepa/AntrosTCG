using System.Collections.Generic;
using ATCG.Capacities;
using UnityEngine;

namespace ATCG
{
    public interface ICapacityDataProvider
    {
        IEnumerable<CapacityData> GetCapacities();
    }
}