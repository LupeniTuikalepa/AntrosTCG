using ATCG.Battle.Entities.Iterations;

namespace ATCG.Battle.Entities.Components.Status
{
    [IteratableComponent]
    public interface IStatusTurnController : IEntityComponent
    {
        void OnTurnStarted();
        void OnTurnEnded();
    }
}