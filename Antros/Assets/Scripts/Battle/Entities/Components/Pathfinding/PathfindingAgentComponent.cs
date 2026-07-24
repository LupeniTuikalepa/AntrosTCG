using System;
using ATCG.Battle.Entities.Components;

namespace ATCG.Battle.Grids
{
    public readonly struct PathfindingAgentComponent : IEntityComponent
    {
        public ReadOnlySpan<PathfindingTraversableRule>  AgentRules => agentRules;

        private readonly PathfindingTraversableRule[] agentRules;

        public readonly AgentMovementType movementType;


        public PathfindingAgentComponent(AgentMovementType movementType, params PathfindingTraversableRule[] agentRules)
        {
            this.movementType = movementType;
            this.agentRules = agentRules;
        }
    }
}