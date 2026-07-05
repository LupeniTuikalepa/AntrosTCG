using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components.Status;
using ATCG.Capacities.Data.Status;

namespace ATCG.Battle.CapacitySystem.Status.Forst
{
    public partial struct FreezeStatus : IStatus<FreezeStatusData>
    {
        public void Apply(FreezeStatusData data, EntityAddress target, StatusContext context)
        {
            target.ApplyStatus(
                new FreezeStatusComponent(data),
                new StatusDurationController<FreezeStatusComponent>(data.Duration),
                context);
        }

        public void Remove(FreezeStatusData data, EntityAddress address, StatusContext context)
        {
            address.RemoveStatus<FreezeStatusComponent>(context);
        }

        public void Tick(FreezeStatusData data, EntityAddress address, StatusContext context)
        {
        }

        public void TickAll(FreezeStatusData data, StatusContext context)
        {
        }
    }
}