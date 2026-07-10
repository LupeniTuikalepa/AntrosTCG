using ATCG.Battle.Entities;
using ATCG.Battle.GameModes;
using ATCG.Battle.Grids;

namespace ATCG.Battle.CapacitySystem.Core.Status
{
    public readonly struct StatusContext
    {
        public World World => battlePhase.world;
        public BattleGrid Grid => battlePhase.BattleGrid;

        public readonly BattlePhase battlePhase;

        public StatusContext(BattlePhase battlePhase)
        {
            this.battlePhase = battlePhase;
        }
    }
}