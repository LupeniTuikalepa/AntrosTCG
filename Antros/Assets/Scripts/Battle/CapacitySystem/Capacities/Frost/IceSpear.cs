using System;
using System.Collections.Generic;
using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Grids;
using ATCG.Capacities.Data.Frost;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns.Building;

namespace ATCG.Battle.CapacitySystem.Capacities.Frost
{
    public partial struct IceSpear : ICapacity<IceSpearData>
    {
        // Valid default: targets the cell and every member on it.
        public void GetTargets(IceSpearData data, BattleCellAspect battleCell, List<EntityAddress> output)
        {
            output.Add(battleCell.EntityAddress);
            foreach (var member in battleCell.GetMembers())
                output.Add(member.EntityAddress);
        }

        public HexPatternBuilder GetHitPattern(IceSpearData data, BattleGrid battleGrid, HexCoordinates castPoint, HexCoordinates casterOrigin)
            => throw new NotImplementedException();

        // Step wired by [WithStep("Hit")] on IceSpearData.
        private partial void ExecuteHit(IceSpearData data, CapacityStepContext ctx)
            => throw new NotImplementedException();
    }
}
