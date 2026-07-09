using ATCG.Battle.Entities.Components;

namespace ATCG.Battle.CapacitySystem.Core.Status
{
    public interface IStatusController : IEntityComponent
    {
        public bool IsFinished();

    }
}