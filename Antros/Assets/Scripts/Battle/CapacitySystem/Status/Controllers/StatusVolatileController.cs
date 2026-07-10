
using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Capacities.Data.Status;

namespace ATCG.Battle.Entities.Components.Status
{
	public struct StatusVolatileController : IStatusController ,IStatusTurnController
    {
        private bool destroyOnNextCheck;
        private bool wasTriggered;


        public void Trigger() => wasTriggered = true;

        bool IStatusController.IsFinished() => destroyOnNextCheck;


        void IStatusTurnController.OnTurnStarted()
        {
            if (!wasTriggered)
                destroyOnNextCheck = true;
        }

        void IStatusTurnController.OnTurnEnded()
        {
            wasTriggered = false;
        }
    }
}