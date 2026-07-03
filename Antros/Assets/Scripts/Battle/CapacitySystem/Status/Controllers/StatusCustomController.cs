using System;
using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Status;

namespace ATCG.Battle.Entities.Components.Status
{
    public readonly struct StatusCustomController<T> : IStatusController<T> where T : struct, IStatusComponent
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