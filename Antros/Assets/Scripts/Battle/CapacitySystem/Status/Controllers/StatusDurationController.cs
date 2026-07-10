using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Status;
using ATCG.Battle.Entities.Components.Implementations;

namespace ATCG.Battle.Entities.Components.Status
{
	public struct StatusDurationController : IStatusController, IStatusTurnController
    {
        public int RemainingTicks { get; private set; }

        public StatusDurationController(int remainingTick)
        {
            this.RemainingTicks = remainingTick;
        }

        public void AddOrRemoveTicks(int ticks)
        {
	        RemainingTicks += ticks;
        }

        bool IStatusController.IsFinished()
        {
            return RemainingTicks <= 0;
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