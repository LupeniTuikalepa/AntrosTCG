using System;
using System.Collections.Generic;
using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Grids;
using ATCG.Battle.Players;
using ATCG.Capacities;
using ATCG.Capacities.Data.Frost;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns.Building;

namespace ATCG.Battle.CapacitySystem.Capacities
{
    public partial struct Crystallization : ICapacity<CrystallizationData>
    {
        // Valid default: targets the cell and every member on it.
        public void GetTargets(CrystallizationData data, BattleCellAspect battleCell, CapacityTargets output, IBattlePlayer castingPlayer)
        {
            output.Add(battleCell.EntityAddress, CapacityTags.CELL);
            foreach (var member in battleCell.GetMembers())
                output.Add(member.EntityAddress, CapacityTags.MEMBER);
        }

        public void GetHitPattern(CrystallizationData data, ref HexPatternBuilder builder, BattleGrid battleGrid, HexCoordinates castPoint, HexCoordinates casterOrigin)
            => throw new NotImplementedException();

        // Step wired by [WithStep("Crystallize")] on CrystallizationData.
        private partial void ExecuteCrystallize(CrystallizationData data, CapacityStepContext ctx)
            => throw new NotImplementedException();
    }
}