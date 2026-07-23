using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Status.Controllers;
using ATCG.Capacities.Data.Status;

namespace ATCG.Battle.CapacitySystem.Status.Frost
{
    public partial class FreezeStatus : Status<FreezeStatusData, StatusDurationController>
    {
        protected override StatusDurationController CreateStatusController(FreezeStatusData data, in StatusContext context)
        {
            return new StatusDurationController(data.Duration);
        }
        
    }
}