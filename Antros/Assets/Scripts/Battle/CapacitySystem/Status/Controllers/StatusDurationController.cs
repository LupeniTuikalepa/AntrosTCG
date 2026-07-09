using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Status;
using ATCG.Battle.Entities.Components.Implementations;

namespace ATCG.Battle.Entities.Components.Status
{
	public struct StatusDurationController : IStatusController, IStatusTurnController
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

        bool IStatusController.IsFinished()
        {
            return remainingTick <= 0;
        }

        void IStatusTurnController.OnTurnStarted()
        {

        }

        void IStatusTurnController.OnTurnEnded()
        {
	        AddOrRemoveTicks(-1);
        }
    }
}