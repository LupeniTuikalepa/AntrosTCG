using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Iterations;

namespace ATCG.Battle.CapacitySystem.Core.Status
{
    [IteratableComponent]
    public interface IStatusController : IEntityComponent
    {
        public bool IsFinished();

    }
}