using ATCG.Battle.GameModes;

namespace ATCG.Battle.Entities.Components.Status
{
    public interface IStatusComponent: IEntityComponent
    {
        void Trigger(EntityAddress address, BattlePhase battlePhase);
    }
}