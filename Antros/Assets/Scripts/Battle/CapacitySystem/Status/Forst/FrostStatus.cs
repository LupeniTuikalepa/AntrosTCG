using ATCG.Battle.Capacities.Status;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components.Status;
using ATCG.Capacities.Data.Status;

namespace ATCG.Battle.CapacitySystem.Status.Forst
{
    public partial struct FrostStatus : IStatus<FrostStatusData>
    {
        public void Apply(FrostStatusData data, EntityAddress target, StatusContext context)
        {
            target.ApplyStatus(
                new FrostStatusComponent(data),
                new StatusDurationController<FrostStatusComponent>(data.Duration),
                context);
        }

        public void Remove(FrostStatusData data, EntityAddress address, StatusContext context)
        {
            address.RemoveStatus<FrostStatusComponent>(context);
        }

        public void Tick(FrostStatusData data, EntityAddress address, StatusContext context)
        {
        }

        public void TickAll(FrostStatusData data, StatusContext context)
        {
        }
    }
}