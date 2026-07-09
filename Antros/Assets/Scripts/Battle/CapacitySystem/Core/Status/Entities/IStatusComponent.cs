using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Iterations;
using ATCG.Capacities.Data.Status;

namespace ATCG.Battle.CapacitySystem.Core.Status
{
    [IteratableComponent]
    public interface IStatusComponent : IEntityComponent
    {
        public StatusData StatusData { get; }
    }
}