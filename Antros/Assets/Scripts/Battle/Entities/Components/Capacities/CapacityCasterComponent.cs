using System.Collections.Generic;
using ATCG.Capacities;
using UnityEngine;

namespace ATCG.Battle.Entities.Components
{
    public readonly struct CapacityCasterComponent : IEntityComponent
    {
        public readonly HashSet<CapacityData> capacities;

        public CapacityCasterComponent(HashSet<CapacityData>capacities)
        {
            this.capacities = capacities;
        }

        public void AddCapacity(CapacityData capacity)
        {
	        Debug.Log($"Capacity Caster: {capacity}");
	        capacities.Add(capacity);
        }

        public void RemoveCapacity(CapacityData capacity)
        {
	        capacities.Remove(capacity);
        }
    }
}