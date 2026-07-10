using ATCG.Capacities.Data.Status;

namespace ATCG.Battle.CapacitySystem.Core.Status
{
    public readonly struct StatusSignature<T> : IStatusComponent where T : IStatus
    {
        public StatusData StatusData { get; }

        public StatusSignature(StatusData statusData)
        {
            StatusData = statusData;
        }
    }
}