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
using ATCG.HexGrids.Utility;

namespace ATCG.Battle.CapacitySystem.Capacities.Frost
{
    public partial struct IceSpear : ICapacity<IceSpearData>
    {
        public void GetTargets(IceSpearData data, BattleCellAspect battleCell, CapacityTargets output, IBattlePlayer castingPlayer)
        {
            foreach (var member in battleCell.GetMembers())
                output.Add(member.EntityAddress, CapacityTags.MEMBER);
        }

        public void GetHitPattern(IceSpearData data, ref HexPatternBuilder builder, BattleGrid battleGrid, HexCoordinates castPoint, HexCoordinates casterOrigin)
        {
            int distance = data.MaxDistance;
            HexCoordinates direction = casterOrigin.GetNormalizedDirection(castPoint);
            HexCoordinates hit = casterOrigin + direction * distance;

            for (int i = 1; i < distance; i++)
            {
                HexCoordinates current = casterOrigin + direction * i;
                if (battleGrid.TryGetBattleCell(current, out var cell))
                {
                    if (cell.HasPhysicalMember())
                    {
                        hit = current;
                        break;
                    }
                }
            }

            builder = builder.With(hit);
        }
        // Step wired by [WithStep("Hit")] on IceSpearData.
        private partial void ExecuteHit(IceSpearData data, CapacityStepContext ctx)
            => throw new NotImplementedException();

    }
}