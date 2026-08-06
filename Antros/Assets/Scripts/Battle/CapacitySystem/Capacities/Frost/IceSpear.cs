using System;
using System.Collections.Generic;
using System.Linq;
using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Grids;
using ATCG.Battle.Players;
using ATCG.Capacities;
using ATCG.Capacities.Attributs;
using ATCG.Capacities.Data.Frost;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns.Building;
using ATCG.HexGrids.Utility;
using ATCG.Utilities;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Capacities.Frost
{
    public partial struct IceSpear : ICapacity<IceSpearData>
    {
        [CapacityPropertyKey]
        public const string HIT_DISTANCE_PROPERTY = "HIT_DISTANCE";

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
        {
            if (!ctx.Targets.Any())
                ctx.capacityPhase.InjectProperty(HIT_DISTANCE_PROPERTY, data.MaxDistance);

            int hitDistance = int.MinValue;

            foreach (var target in ctx.Targets)
            {
                if (target.TryGetComponentRO(out GridMemberComponent gridMemberComponent))
                {
                    HexCoordinates coordinates = gridMemberComponent.coordinates;
                    int distance = coordinates.Distance(ctx.CasterOrigin);
                    float t = Mathf.InverseLerp(data.MinDistance, data.MaxDistance, distance);

                    int dmg = GameMaths.Round(Mathf.Lerp(data.MinDamage, data.MaxDamage, t));
                    hitDistance = hitDistance < distance ? distance : hitDistance;

                    DamageCommand damageCommand = new DamageCommand(dmg, target);
                    damageCommand.Run(ctx.BattlePhase);
                }
            }

            //Size of the blade is for the furthest target
            ctx.capacityPhase.InjectProperty(HIT_DISTANCE_PROPERTY, hitDistance);
        }

    }
}