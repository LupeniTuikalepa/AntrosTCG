using ATCG.Battle.Entities.Aspects;
using ATCG.HexGrids;

namespace ATCG.Battle.Grids
{
    public struct PathfindingRedirectionIterator : IRedirectStatusComponentIterator
    {
        public HexCoordinates From { get; }
        public HexCoordinates To { get; private set; }

        public AgentMovementType SegmentType { get; private set; }

        public bool WasRedirected { get; private set; }

        public BattleCellAspect battleCellAspect;
        public PathfindingAgentAspect agent;

        public PathfindingRedirectionIterator(BattleCellAspect from, HexCoordinates to,  PathfindingAgentAspect agent)
        {
            this.battleCellAspect = from;
            this.agent = agent;

            From = from.Coordinate;
            To = to;
            SegmentType = agent.MovementType;
            WasRedirected = false;
        }


        public void Process<T>() where T : struct, IRedirectStatusComponent
        {
            if(!WasRedirected)
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