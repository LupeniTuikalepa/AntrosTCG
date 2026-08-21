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
        public CapacityTargets Targets => targets;
        public bool HasCaster => Caster.IsValid;
        public BattlePhase BattlePhase => capacityPhase.battlePhase;
        public CapacityData Data => capacityPhase.data;
        public HexCoordinates CastPoint => capacityPhase.castPoint;
        public HexCoordinates CasterOrigin => capacityPhase.CasterOrigin;
        public EntityAddress Caster => capacityPhase.caster;
        public BattleGrid BattleGrid => capacityPhase.battlePhase.BattleGrid;

        public  BattleID CastingPlayer => capacityPhase.casterPlayerId;

        public readonly int loop;
        public readonly CastCapacityPhase capacityPhase;
        public readonly CapacityStepData stepData;
        public readonly float effectiveness;
        public readonly HexPatternBuilder patternBuilder;

        private readonly CapacityTargets targets;


        public CapacityStepContext(CastCapacityPhase capacityPhase, float effectiveness, CapacityStepData stepData, CapacityTargets targets, HexPatternBuilder patternBuilder, int loop)
        {
            this.capacityPhase = capacityPhase;
            this.effectiveness = effectiveness;
            this.stepData = stepData;
            this.targets = targets;
            this.patternBuilder = patternBuilder;
            this.loop = loop;
        }


        public bool IsAlly(EntityAddress address)
        {
	        if (!Caster.TryGetComponentRO(out BelongsToPlayerComponent casterBelongsToPlayer))
		        return false;
	        
	        if (address.TryGetComponentRO(out BelongsToPlayerComponent targetBelongsToPlayer))
		        return targetBelongsToPlayer.IsAllieOf(casterBelongsToPlayer.playerId);

            return false;
        }
    }
}