using ATCG.Enums;

namespace ATCG.Battle.Grids
{
    public enum AgentMovementType
    {
        Default,
        Slide,
        Push,
        Jump,
        Flight,
        Teleportation,
    }

    public static class AgentMovementTypeExtensions
    {
        public static AgentMovementType ToAgentMovementType(this MovementType movementType)
        {
            return movementType switch
            {
                MovementType.Walk => AgentMovementType.Default,
                MovementType.Flight => AgentMovementType.Flight,
                MovementType.Teleportation => AgentMovementType.Teleportation,
                _  => AgentMovementType.Default
            };
        }
    }
}