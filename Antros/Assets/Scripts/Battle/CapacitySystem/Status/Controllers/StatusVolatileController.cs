
using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Capacities.Data.Status;

namespace ATCG.Battle.Entities.Components.Status
{
	public struct StatusVolatileController : IStatusController ,IStatusTurnController
    {
        private bool destroyOnTurnEnd;

        public StatusVolatileController(bool destroyOnTurnEnd)
        {
            this.destroyOnTurnEnd = destroyOnTurnEnd;
        }

        public void Reset()
        {
	        destroyOnTurnEnd =  true;
        }

        public void Trigger() => destroyOnTurnEnd = false;

        bool IStatusController.IsFinished() => destroyOnTurnEnd;


        void IStatusTurnController.OnTurnStarted()
        {

        }

        void IStatusTurnController.OnTurnEnded()
        {
	        Reset();
        }
    }
}