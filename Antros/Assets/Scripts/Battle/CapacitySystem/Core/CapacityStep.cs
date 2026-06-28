using ATCG.Battle.Commands.GameCommands;
using ATCG.Capacities;

namespace ATCG.Battle.CapacitySystem.Capacities
{
    public class CapacityStep<T> : ICapacityStep where T : CapacityData
    {
        public delegate void CapacityStepDelegate(T data, CapacityStepContext stepContext);

        public string StepName { get; }

        public readonly CapacityStepDelegate callback;

        public readonly T data;

        public CapacityStep(T data, CapacityStepDelegate callback, string stepName)
        {
            this.data = data;
            this.callback = callback;
            StepName = stepName;
        }

        public void RunStep(in CapacityStepContext stepContext) => callback(data, stepContext);
    }
}