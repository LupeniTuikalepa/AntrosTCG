using ATCG.Battle.GameModes;

namespace ATCG.Battle.Entities.Components.Status
{
    public readonly struct StatusContext
    {
        public readonly BattlePhase battlePhase;

        public StatusContext(BattlePhase battlePhase)
        {
            this.battlePhase = battlePhase;
        }
    }
}