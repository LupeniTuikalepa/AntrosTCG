using System;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Grids;

namespace ATCG.Battle.Entities.Aspects
{
    public partial struct PathfindingAgentAspect : IEntityAspect<PathfindingAgentComponent, GridMemberComponent>
    {
        public AgentMovementType MovementType => PathfindingAgentComponent.movementType;
        public ReadOnlySpan<PathfindingTraversableRule> AgentRules => PathfindingAgentComponent.AgentRules;

    }

}