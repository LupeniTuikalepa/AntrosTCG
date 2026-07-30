using ATCG.Capacities.Data.Status;

namespace ATCG.Battle.CapacitySystem.Core.Status
{
    public struct DefaultStatusComponent<TData> : IStatusComponent where TData : StatusData
    {
        public StatusData StatusStatusData { get; }

        public DefaultStatusComponent(TData statusStatusData)
        {
            StatusStatusData = statusStatusData;
        }

    }
}