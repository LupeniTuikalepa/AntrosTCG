using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Status.Controllers;
using ATCG.Capacities.Data.Status;

namespace ATCG.Battle.CapacitySystem.Status.Frost
{
    public partial class BlackIceStatus : Status<BlackIceStatusData, BlackIceStatusComponent, StatusDurationController>
    {
        protected override BlackIceStatusComponent CreateStatusComponent(BlackIceStatusData data, in StatusContext context)
        {
            return new BlackIceStatusComponent(data);
        }

        protected override StatusDurationController CreateStatusController(BlackIceStatusData data, in StatusContext context)
        {
            return new StatusDurationController(data.Duration);
        }

    }
}