using ATCG.Capacities.Data.Status;

namespace ATCG.Battle.Entities.Runtime.Status
{
    public interface IRuntimeStatusComponent
    {
        void OnApplyStatus(StatusData statusData);
        void OnRemoveStatus();
        void OnTickStatus(RuntimeStatusContext context);
    }
}