using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.GameModes;
using ATCG.Capacities;
using ATCG.HexGrids;

namespace ATCG.Battle.CapacitySystem.Core
{
    public readonly struct CapacityStepContext
    {
        public readonly CastCapacityPhase capacityPhase;
        public readonly CapacityStepData stepData;
        public readonly float effectiveness;

        public bool HasCaster => Caster.IsValid;
        public BattlePhase BattlePhase => capacityPhase.battlePhase;
        public CapacityData Data => capacityPhase.data;
        public HexCoordinates CastPoint => capacityPhase.castPoint;
        public EntityAddress Caster => capacityPhase.caster;

        public  BattleID CastingPlayer => capacityPhase.casterPlayerId;

        public CapacityStepContext(CastCapacityPhase capacityPhase, float effectiveness, CapacityStepData stepData)
        {
            this.capacityPhase = capacityPhase;
            this.effectiveness = effectiveness;
            this.stepData = stepData;
        }


        public bool IsAlly(EntityAddress address)
        {
            if (address.TryGetComponentRO(out BelongsToPlayerComponent otherBelongsToPlayer))
                return otherBelongsToPlayer.IsAllieOf(CastingPlayer);

            return false;
        }
    }
}