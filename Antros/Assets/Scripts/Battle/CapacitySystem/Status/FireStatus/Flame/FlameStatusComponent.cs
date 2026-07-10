using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Capacities.Data.Status;


namespace ATCG.Battle
{
    public readonly struct FlameStatusComponent : IStatusComponent
    {
	    StatusData IStatusComponent.StatusData => data;
	    private readonly FlameStatusData data;

	    public FlameStatusComponent(FlameStatusData data)
	    {
		    this.data = data;
	    }
    }
}