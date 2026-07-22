using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Capacities.Data.Status;
using ATCG.Capacities.Status.FireStatus;

namespace ATCG.Battle.CapacitySystem.Status.FireStatus.Fournaise
{
	public readonly struct FurnaceComponent : IStatusComponent
	{
		public readonly FurnaceData data;
		public StatusData StatusData => data;
		
		public FurnaceComponent(FurnaceData data)
		{
			this.data = data;
		}
	}
}