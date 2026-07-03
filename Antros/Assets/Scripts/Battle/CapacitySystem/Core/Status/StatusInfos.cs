using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Capacities.Data.Status;

namespace ATCG.Battle.CapacitySystem.Core.Status
{
    public readonly struct StatusInfos<TStatus> : IEntityComponent where TStatus : struct, IStatusComponent
    {
        public readonly ComponentMask componentMask;
        public readonly StatusData statusData;

        public StatusInfos(ComponentMask componentMask, StatusData statusData)
        {
            this.componentMask = componentMask;
            this.statusData = statusData;
        }
    }
}