using System.Collections.Generic;
using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Capacities.Data.Status;

namespace ATCG.Battle.Entities.Components
{
    public readonly struct StatusReceiver : IEntityComponent
    {
        public IEnumerable<ComponentRef<StatusTag>> AllStatus => allStatus.Values;

        private readonly Dictionary<Entity, ComponentRef<StatusTag>> allStatus;

        public StatusReceiver(int capacity = 32)
        {
            allStatus = new(capacity);
        }


        public void RegisterStatus(ComponentRef<StatusTag> status)
        {
            allStatus.Add(status.Entity, status);
        }

        public void UnregisterStatus(ComponentRef<StatusTag> status)
        {
            allStatus.Remove(status.Entity);
        }

        public bool Has<TData>(out ComponentRef<StatusTag> componentRef) where TData : StatusData
        {
            foreach (var s in allStatus.Values)
            {
                if (s.GetValue().data is TData)
                {
                    componentRef = s;
                    return true;
                }
            }

            componentRef = default;
            return false;
        }
    }
}