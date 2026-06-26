using ATCG.Battle.GameModes;
using ATCG.Capacities.Data.Status;

namespace ATCG.Battle.Entities.Components.Status
{
    public interface IStatusComponent: IEntityComponent
    {
        public StatusData StatusData { get; }
        void Trigger(EntityAddress address, BattlePhase battlePhase);
    }
}