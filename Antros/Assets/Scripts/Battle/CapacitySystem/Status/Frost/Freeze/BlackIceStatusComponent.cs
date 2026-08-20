using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Grids;
using ATCG.Capacities.Data.Status;
using ATCG.HexGrids;
using ATCG.HexGrids.Utility;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Status.Frost
{
    public struct BlackIceStatusComponent : IRedirectStatusComponent
    {
        public StatusData StatusStatusData { get; }

        public BlackIceStatusComponent(StatusData statusStatusData)
        {
            StatusStatusData = statusStatusData;
        }


        public bool TryRedirect(PathfindingAgentAspect aspect, HexCoordinates from, ref HexCoordinates to, ref AgentMovementType agentMovementType)
        {
            var direction = from.GetNormalizedDirection(to).NearestCardinal();

            if (direction is { X: 0, Y: 0 })
                return false;

            to += direction;
            return true;
        }
    }
}