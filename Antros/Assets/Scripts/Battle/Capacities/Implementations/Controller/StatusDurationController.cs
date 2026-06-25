using ATCG.Battle.Entities.Components.Implementations;

namespace ATCG.Battle.Entities.Components.Status
{
    public struct StatusDurationController<T> : IStatusController<T> where T : struct, IStatusComponent
    {
        private int remainingTick;

        public StatusDurationController(int remainingTick)
        {
            this.remainingTick = remainingTick;
        }

        public bool IsFinished(ComponentRef<T> componentRef)
        {
            remainingTick--;
            return remainingTick <= 0;
            
        }
    }
}