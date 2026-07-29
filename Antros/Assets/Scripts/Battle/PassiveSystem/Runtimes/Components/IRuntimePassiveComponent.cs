namespace ATCG.Battle.PassiveSystem.Runtimes.Components
{
    public interface IRuntimePassiveComponent
    {
        void OnApplyPassive(RuntimePassiveContext context);
        void OnRemovePassive(RuntimePassiveContext context);
        void OnTickPassive(RuntimePassiveContext context);
    }
}