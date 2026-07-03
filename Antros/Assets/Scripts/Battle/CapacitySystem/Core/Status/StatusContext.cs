using ATCG.Battle.GameModes;

namespace ATCG.Battle.CapacitySystem.Core.Status
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