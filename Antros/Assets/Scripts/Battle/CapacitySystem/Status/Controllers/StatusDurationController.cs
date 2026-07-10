using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Status.Iterations;

namespace ATCG.Battle.Entities.Components.Status
{
	public struct StatusDurationController : IStatusController, IUpdateControllerOnTurnEnd
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


        void IUpdateControllerOnTurnEnd.Process()
        {
	        AddOrRemoveTicks(-1);
        }
    }
}