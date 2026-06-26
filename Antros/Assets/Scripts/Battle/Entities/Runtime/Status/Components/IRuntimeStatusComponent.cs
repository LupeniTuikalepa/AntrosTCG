namespace ATCG.Battle.Entities.Runtime.Status
{
    public interface IRuntimeStatusComponent
    {
        void OnApplyStatus();
        void OnRemoveStatus();
        void OnTickStatus(RuntimeStatusContext context);
    }
}