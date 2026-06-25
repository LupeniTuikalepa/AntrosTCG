using System;
using ATCG.Battle.Capacities.Status;
using ATCG.Battle.Entities.Components.Status;
using ATCG.Capacities.Data.Status;
using UnityEngine;

namespace ATCG.Battle.Entities.Components.Implementations
{
    public partial struct PoisonStatus : IStatus<PoisonStatusData>
    {
        public void Apply(PoisonStatusData data, EntityAddress target, StatusContext context)
        {
            target.ApplyStatus(new PoisonStatusComponent(data.Damage), 
                new StatusDurationController<PoisonStatusComponent>(data.Duration),
                context);
        }

        public void Remove(PoisonStatusData data, EntityAddress address, StatusContext context)
        {
            address.RemoveStatus<PoisonStatusComponent>(context);
        }

        public void Tick(PoisonStatusData data, EntityAddress address, StatusContext context)
        {
            StatusManager.Trigger<PoisonStatusComponent>(address, context);
        }

        public void TickAll(PoisonStatusData data, StatusContext context)
        {
            StatusManager.ProcessAllStatus<PoisonStatusComponent>(context);
        }
    }
}