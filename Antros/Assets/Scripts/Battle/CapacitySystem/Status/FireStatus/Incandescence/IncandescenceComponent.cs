using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Capacities.Data.Status;
using ATCG.Capacities.Status.FireStatus;

namespace ATCG.Battle.CapacitySystem.Status
{
	public readonly struct IncandescenceComponent : IStatusComponent
	{
		public readonly IncandescenceStatusData statusData;
		public StatusData StatusStatusData => statusData;

		public IncandescenceComponent(IncandescenceStatusData statusData)
		{
			this.statusData = statusData;
		}

	}
}