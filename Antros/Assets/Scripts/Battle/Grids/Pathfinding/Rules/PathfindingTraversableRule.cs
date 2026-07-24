using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.HexGrids;

namespace ATCG.Battle.Grids
{
    public abstract class PathfindingTraversableRule
    {
        public abstract bool CanTraverse(PathfindingAgentAspect agent, BattleCellAspect battleCellAspect);
    }
}