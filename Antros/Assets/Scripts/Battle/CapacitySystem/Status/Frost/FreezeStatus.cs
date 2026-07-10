using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components.Status;
using ATCG.Capacities.Data.Status;

namespace ATCG.Battle.CapacitySystem.Status.Forst
{
    public partial class FreezeStatus : Status<FreezeStatusData, StatusDurationController>
    {
        protected override StatusDurationController CreateStatusController(FreezeStatusData data, in StatusContext context)
        {
            return new StatusDurationController(data.Duration);
        }
        
    }
}