using ATCG.Battle.Entities.Components;
using ATCG.Battle.Utilities.Iterations;
using ATCG.Capacities.Data.Status;

namespace ATCG.Battle.CapacitySystem.Core.Status
{
    [GenerateComponentIterator]
    public interface IStatusComponent : IEntityComponent
    {
        public StatusData StatusStatusData { get; }
    }
}