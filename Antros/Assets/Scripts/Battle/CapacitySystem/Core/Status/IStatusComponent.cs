using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Iterations;
using ATCG.Battle.GameModes;
using ATCG.Capacities.Data.Status;

namespace ATCG.Battle.CapacitySystem.Core.Status
{
    [IteratableComponent]
    public interface IStatusComponent : IEntityComponent
    {
        public StatusData StatusData { get; }
        void Trigger(EntityAddress address, BattlePhase battlePhase);
    }
}