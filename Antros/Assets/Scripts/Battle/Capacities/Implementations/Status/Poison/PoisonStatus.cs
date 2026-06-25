using ATCG.Battle.Capacities.Status;
using ATCG.Battle.Entities.Components.Status;
using ATCG.Capacities.Data.Status;
using UnityEngine;

namespace ATCG.Battle.Entities.Components.Implementations
{
    public partial struct PoisonStatus : IStatus<PoisonStatusData>
    {
        public void Apply(PoisonStatusData data, EntityAddress target)
        {
            StatusManager.ApplyStatus(target, 
                new PoisonStatusComponent(data.Damage), 
                new StatusDurationController<PoisonStatusComponent>(data.Duration));
        }
    }
}