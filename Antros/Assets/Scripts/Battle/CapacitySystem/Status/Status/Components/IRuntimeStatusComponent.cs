namespace ATCG.Battle.CapacitySystem.Status.Status.Components
{
    public interface IRuntimeStatusComponent
    {
        void OnApplyStatus(RuntimeStatusContext context);
        void OnRemoveStatus(RuntimeStatusContext context);
        void OnTickStatus(RuntimeStatusContext context);
    }
}