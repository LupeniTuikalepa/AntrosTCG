using ATCG.Capacities;

namespace ATCG.Battle.CapacitySystem.Core
{
    public class CapacityStep<T> : ICapacityStep where T : CapacityData
    {
        public delegate void CapacityStepDelegate(T data, CapacityStepContext stepContext);

        public string StepName { get; }

        public readonly CapacityStepDelegate callback;

        public readonly T data;

        public CapacityStep(string stepName)
        {
            this.StepName = stepName;
            this.data = null;
            this.callback = null;
        }
        public CapacityStep(T data, CapacityStepDelegate callback, string stepName)
        {
            this.data = data;
            this.callback = callback;
            StepName = stepName;
        }

        public void RunStep(in CapacityStepContext ctx) => callback?.Invoke(data, ctx);
    }
}