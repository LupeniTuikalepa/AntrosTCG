using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Capacities.Data.Status;
using ATCG.Capacities.Status.FireStatus;

namespace ATCG.Battle.CapacitySystem.Status
{
	public readonly struct IncandescenceComponent : IStatusComponent
	{
		public readonly IncandescenceData data;
		public StatusData StatusData => data;

		public IncandescenceComponent(IncandescenceData data)
		{
			this.data = data;
		}

	}
}