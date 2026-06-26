using ATCG.Capacities.Data.Status;

namespace ATCG.Battle.Entities.Components.Status
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