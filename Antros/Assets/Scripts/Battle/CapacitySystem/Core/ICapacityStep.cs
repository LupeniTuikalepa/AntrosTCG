using ATCG.Battle.Commands.GameCommands;

namespace ATCG.Battle.CapacitySystem.Capacities
{
    public interface ICapacityStep
    {
        public string StepName { get; }
        void RunStep(in CapacityStepContext stepContext);
    }
}