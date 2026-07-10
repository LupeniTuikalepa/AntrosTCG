using ATCG.Battle.Entities.Components;
using ATCG.Battle.Utilities.Iterations;

namespace ATCG.Battle.CapacitySystem.Core.Status
{
    [GenerateComponentIterator]
    public interface IStatusController : IEntityComponent
    {
        public bool IsFinished();

    }
}