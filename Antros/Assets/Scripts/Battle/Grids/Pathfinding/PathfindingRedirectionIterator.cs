using ATCG.Battle.Entities.Aspects;
using ATCG.HexGrids;
using UnityEngine;

namespace ATCG.Battle.Grids
{
    public struct PathfindingRedirectionIterator : IRedirectStatusComponentIterator
    {
        // Origin used to compute the redirect direction (e.g. Black Ice slides you further
        // along this -> To). It is NOT necessarily the cell owning the redirect component.
        public HexCoordinates From { get; }
        public HexCoordinates To { get; private set; }

        public AgentMovementType SegmentType { get; private set; }

        public bool WasRedirected { get; private set; }

        // The cell whose redirect status components are queried.
        public BattleCellAspect battleCellAspect;
        public PathfindingAgentAspect agent;

        // Legacy ctor: direction origin defaults to the redirect cell's own coordinate.
        public PathfindingRedirectionIterator(BattleCellAspect from, HexCoordinates to, PathfindingAgentAspect agent)
            : this(from, from.Coordinate, to, agent)
        {
        }

        // The redirect component lives on `redirectCell` (the tile you step ONTO); the push
        // direction is directionOrigin -> to (i.e. the direction you entered from), so a slide
        // continues along your movement.
        public PathfindingRedirectionIterator(BattleCellAspect redirectCell, HexCoordinates directionOrigin, HexCoordinates to, PathfindingAgentAspect agent)
        {
            this.battleCellAspect = redirectCell;
            this.agent = agent;

            From = directionOrigin;
            To = to;
            SegmentType = agent.MovementType;
            WasRedirected = false;
        }


        public void Process<T>() where T : struct, IRedirectStatusComponent
        {
            Debug.Log($"[PathfindingIterator] Processing");
            // Only one redirect component may fire per pass: once one has redirected, skip
            // the rest. (Was inverted — `if(!WasRedirected) return;` — so the first component
            // always returned early and no redirect ever happened.)
            if (WasRedirected)
                return;

            if (battleCellAspect.EntityAddress.TryGetComponentRO<T>(out var redirectStatusComponent))
            {
                AgentMovementType agentMovementType = SegmentType;
                HexCoordinates hexCoordinates = To;

                if (redirectStatusComponent.TryRedirect(agent, From, ref hexCoordinates, ref agentMovementType))
                {
                    To = hexCoordinates;
                    SegmentType = agentMovementType;
                    WasRedirected = true;
                }
            }
        }
    }
}
