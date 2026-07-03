using ATCG.Battle.Entities.Components;

namespace ATCG.Battle.CapacitySystem.Core.Status
{
    public interface IStatusController<T> : IEntityComponent where T : struct, IStatusComponent
    {
        public bool IsFinished(ComponentRef<T> componentRef);

    }
}