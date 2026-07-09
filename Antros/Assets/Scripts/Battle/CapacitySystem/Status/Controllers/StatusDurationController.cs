using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Status;
using ATCG.Battle.Entities.Components.Implementations;
using ATCG.Battle.Entities.Iterations;

namespace ATCG.Battle.Entities.Components.Status
{
	[IteratableComponent]
	public interface IStatusTurnController
	{
		void OnTurnStarted();
		void OnTurnEnded();
	}
    public struct StatusDurationController<T> : IStatusController<T>, IStatusTurnController where T : struct, IStatusComponent
    {
        private int remainingTick;
        public int RemainingTicks => remainingTick;

        public StatusDurationController(int remainingTick)
        {
            this.remainingTick = remainingTick;
        }

        public void AddOrRemoveTicks(int ticks)
        {
	        remainingTick += ticks;
        }

        public bool IsFinished(ComponentRef<T> componentRef)
        {
            return remainingTick <= 0;
        }

        public void OnTurnStarted()
        {
	        
        }

        public void OnTurnEnded()
        {
	        AddOrRemoveTicks(-1);
        }
    }
}