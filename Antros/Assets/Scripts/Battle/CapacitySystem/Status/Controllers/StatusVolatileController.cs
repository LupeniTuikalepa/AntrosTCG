
using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Capacities.Data.Status;

namespace ATCG.Battle.Entities.Components.Status
{
	public struct StatusVolatileController<T>: IStatusController<T>,IStatusTurnController where T : struct, IStatusComponent
    {
        private bool willLast;
        
        public void Reset()
        {
	        willLast =  false;
        }

        public void Trigger()
        {
	        willLast = true;
        }

        public bool IsFinished(ComponentRef<T> componentRef)
        {
            return !willLast;
        }

        public void OnTurnStarted()
        {
	        
        }

        public void OnTurnEnded()
        {
	        Reset();
        }
    }
}