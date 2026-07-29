namespace ATCG.Battle.PassiveSystem.Runtimes.Components
{
    public abstract class RuntimePassiveComponent : IRuntimePassiveComponent
    {
        public abstract void OnApplyPassive(RuntimePassiveContext context);
        public abstract void OnRemovePassive(RuntimePassiveContext context);
        public abstract void OnTickPassive(RuntimePassiveContext context);
    }
}