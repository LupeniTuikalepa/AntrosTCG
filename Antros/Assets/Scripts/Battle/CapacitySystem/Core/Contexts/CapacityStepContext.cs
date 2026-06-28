using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.Entities;
using ATCG.Battle.GameModes;
using ATCG.Battle.Grids;
using ATCG.Capacities;
using ATCG.HexGrids;

namespace ATCG.Battle.Commands.GameCommands
{
    public readonly struct CapacityStepContext
    {
        public readonly CastCapacityPhase capacityPhase;
        public readonly CapacityStepData stepData;
        private readonly float effectiveness;

        public BattlePhase BattlePhase => capacityPhase.battlePhase;
        public CapacityData Data => capacityPhase.data;
        public HexCoordinates CastPoint => capacityPhase.castPoint;
        public EntityAddress Caster => capacityPhase.caster;

        public CapacityStepContext(CastCapacityPhase capacityPhase, float effectiveness, CapacityStepData stepData)
        {
            this.capacityPhase = capacityPhase;
            this.effectiveness = effectiveness;
            this.stepData = stepData;
        }

    }
}