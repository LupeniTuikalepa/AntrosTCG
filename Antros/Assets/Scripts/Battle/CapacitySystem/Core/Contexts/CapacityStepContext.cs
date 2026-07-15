using System.Collections.Generic;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.GameModes;
using ATCG.Battle.Grids;
using ATCG.Capacities;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns.Building;

namespace ATCG.Battle.CapacitySystem.Core
{
    public readonly struct CapacityStepContext
    {
        public IReadOnlyList<CapacityTarget> Targets => targets;
        public bool HasCaster => Caster.IsValid;
        public BattlePhase BattlePhase => capacityPhase.battlePhase;
        public CapacityData Data => capacityPhase.data;
        public HexCoordinates CastPoint => capacityPhase.castPoint;
        public HexCoordinates CasterOrigin => capacityPhase.CasterOrigin;
        public EntityAddress Caster => capacityPhase.caster;
        public BattleGrid BattleGrid => capacityPhase.battlePhase.BattleGrid;

        public  BattleID CastingPlayer => capacityPhase.casterPlayerId;

        public readonly CastCapacityPhase capacityPhase;
        public readonly CapacityStepData stepData;
        public readonly float effectiveness;
        public readonly HexPatternBuilder patternBuilder;

        private readonly List<CapacityTarget> targets;


        public CapacityStepContext(CastCapacityPhase capacityPhase, float effectiveness, CapacityStepData stepData,
            List<CapacityTarget> targets, HexPatternBuilder patternBuilder)
        {
            this.capacityPhase = capacityPhase;
            this.effectiveness = effectiveness;
            this.stepData = stepData;
            this.targets = targets;
            this.patternBuilder = patternBuilder;
        }


        public bool IsAlly(EntityAddress address)
        {
            if (address.TryGetComponentRO(out BelongsToPlayerComponent otherBelongsToPlayer))
                return otherBelongsToPlayer.IsAllieOf(CastingPlayer);

            return false;
        }
    }
}