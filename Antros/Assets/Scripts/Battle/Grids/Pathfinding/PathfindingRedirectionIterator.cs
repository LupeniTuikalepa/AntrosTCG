using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.HexGrids;

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

        // NOTE: this iterator is a struct, but the generated ForeachRedirectStatusComponent takes it
        // as an interface — so it runs on a BOXED copy. Callers must box once and read the result
        // back from that same box (see HexPathfinder.TryRedirectOnce); a fresh struct local would
        // lose every mutation made here.
        public void Process<T>() where T : struct, IRedirectStatusComponent
        {
            // Only one redirect component may fire per pass: once one has redirected, skip the rest.
            if (WasRedirected)
                return;

            // A status (Black Ice, and later teleporters) does NOT live on the cell itself: applying
            // it spawns a dedicated status entity that the cell tracks through its StatusReceiver.
            // So we look for T on each of the cell's status entities, not on the cell entity.
            if (!battleCellAspect.EntityAddress.TryGetComponentRO(out StatusReceiver statusReceiver))
                return;

            foreach (ComponentRef<StatusTag> statusTagRef in statusReceiver.AllStatus)
            {
                if (!statusTagRef.EntityAddress.TryGetComponentRO<T>(out var redirectStatusComponent))
                    continue;

                AgentMovementType agentMovementType = SegmentType;
                HexCoordinates hexCoordinates = To;

                if (redirectStatusComponent.TryRedirect(agent, From, ref hexCoordinates, ref agentMovementType))
                {
                    To = hexCoordinates;
                    SegmentType = agentMovementType;
                    WasRedirected = true;
                    return;
                }
            }
        }
    }
}
