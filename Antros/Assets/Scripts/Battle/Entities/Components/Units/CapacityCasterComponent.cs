using ATCG.Capacities;

namespace ATCG.Battle.Entities.Components
{
    public struct CapacityCasterComponent : IEntityComponent
    {
        public readonly CapacityData[] capacities;

        public CapacityCasterComponent(CapacityData[] capacities)
        {
            this.capacities = capacities;
        }
    }
}