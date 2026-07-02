using ATCG.Capacities.Data.Status;

namespace ATCG.Battle.Entities.Runtime.Status
{
    public interface IRuntimeStatusComponent
    {
        void OnApplyStatus(RuntimeStatusContext context);
        void OnRemoveStatus(RuntimeStatusContext context);
        void OnTickStatus(RuntimeStatusContext context);
    }
}