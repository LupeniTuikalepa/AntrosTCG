
using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Status.Iterations;

namespace ATCG.Battle.Entities.Components.Status
{
	public struct StatusVolatileController : IStatusController ,IUpdateControllerOnTurnEnd, IUpdateControllerOnTurnBegin
    {
        private bool destroyOnNextCheck;
        private bool wasTriggered;


        public void Trigger() => wasTriggered = true;

        bool IStatusController.IsFinished() => destroyOnNextCheck;


        void IUpdateControllerOnTurnBegin.Process()
        {
            if (!wasTriggered)
                destroyOnNextCheck = true;
        }

        void IUpdateControllerOnTurnEnd.Process()
        {
            wasTriggered = false;
        }
    }
}