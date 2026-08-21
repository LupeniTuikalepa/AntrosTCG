namespace ATCG.Battle.CapacitySystem.Core
{
    public interface ICapacityStep
    {
        public string StepName { get; }
        void RunStep(in CapacityStepContext ctx);
    }
}