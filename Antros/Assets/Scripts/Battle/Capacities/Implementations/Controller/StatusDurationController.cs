namespace ATCG.Battle.Entities.Components.Status
{
    public struct StatusDurationController<T> : IStatusController<T> where T : struct, IStatus
    {
        private int remainingTick;

        public StatusDurationController(int remainingTick)
        {
            this.remainingTick = remainingTick;
        }

        public bool IsFinished(ComponentRef<T> componentRef)
        {
            var component = componentRef.GetValue();
            component.Trigger(componentRef.EntityAddress);
            remainingTick--;
            return remainingTick <= 0;
            
        }
    }
}