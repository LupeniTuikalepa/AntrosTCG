using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Utilities.Iterations;
using ATCG.HexGrids;

namespace ATCG.Battle.Grids
{
    [GenerateComponentIterator]
    public interface IRedirectStatusComponent : IStatusComponent
    {
        bool TryRedirect(PathfindingAgentAspect aspect, HexCoordinates from, ref HexCoordinates to, ref AgentMovementType agentMovementType);
    }
}