using ATCG.Capacities.Data.Status;

namespace ATCG.Battle.CapacitySystem.Core.Status
{
    public struct DefaultStatusComponent<TData> : IStatusComponent where TData : StatusData
    {
        public StatusData StatusData { get; }

        public DefaultStatusComponent(TData statusData)
        {
            StatusData = statusData;
        }

    }
}