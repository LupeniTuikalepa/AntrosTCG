using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Capacities.Data.Status;


namespace ATCG.Battle
{
    public readonly struct BurnStatusComponent : IStatusComponent
    {
	    StatusData IStatusComponent.StatusData => data;
	    private readonly BurnStatusData data;

	    public BurnStatusComponent(BurnStatusData data)
	    {
		    this.data = data;
	    }
    }
}