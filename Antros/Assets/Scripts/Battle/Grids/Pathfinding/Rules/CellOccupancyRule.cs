using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;

namespace ATCG.Battle.Grids
{
    /// <summary>
    /// Blocks tiles physically occupied by ANOTHER unit. The agent's own tile stays traversable
    /// — its own body is the only thing marking that tile occupied — so a back-and-forth path
    /// that routes through the origin is allowed.
    /// </summary>
    public class CellOccupancyRule : PathfindingTraversableRule
    {
        public override bool CanTraverse(PathfindingAgentAspect agent, BattleCellAspect battleCellAspect)
        {
            int agentEntity = agent.EntityAddress.entity.id;

            foreach (ComponentRef<GridMemberComponent> member in battleCellAspect.GetMembers())
            {
                if (member.entityID == agentEntity)
                    continue; // that's us — our own body never blocks us

                if (member.EntityAddress.Is(out GridMemberAspect memberAspect) && memberAspect.IsPhysical)
                    return false;
            }

            return true;
        }
    }
}