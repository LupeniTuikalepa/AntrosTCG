using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Capacities.Data.Status;
using ATCG.Capacities.Status.FireStatus;

namespace ATCG.Battle.CapacitySystem.Status.FireStatus.Fournaise
{
	public readonly struct FournaiseComponent : IStatusComponent
	{
		public readonly FournaiseData data;
		public StatusData StatusData => data;
		
		public FournaiseComponent(FournaiseData data)
		{
			this.data = data;
		}
	}
}