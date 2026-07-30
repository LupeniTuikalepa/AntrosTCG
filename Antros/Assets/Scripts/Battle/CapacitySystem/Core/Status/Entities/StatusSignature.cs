using ATCG.Capacities.Data.Status;

namespace ATCG.Battle.CapacitySystem.Core.Status
{
    public readonly struct StatusSignature<T> : IStatusComponent where T : IStatus
    {
        public StatusData StatusStatusData { get; }

        public StatusSignature(StatusData statusStatusData)
        {
            StatusStatusData = statusStatusData;
        }
    }
}