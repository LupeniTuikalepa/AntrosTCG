using System;

namespace ATCG.Battle.Entities.Components.Status
{
    public readonly struct StatusCustomController<T> : IStatusController<T> where T : struct, IStatus
    {
        private readonly Func<ComponentRef<T>, bool> func;

        public StatusCustomController(Func<ComponentRef<T>, bool> func)
        {
            this.func = func;
        }

        public bool IsFinished(ComponentRef<T> componentRef)
        {
            return func(componentRef);
        }
    }
}