using ATCG.Battle.Grids;

namespace ATCG.Battle.Players.Local.Phases
{
    public readonly struct PathGenerationContext
    {
        public readonly BattleGrid battleGrid;

        public PathGenerationContext(BattleGrid battleGrid)
        {
            this.battleGrid = battleGrid;
        }
    }
}