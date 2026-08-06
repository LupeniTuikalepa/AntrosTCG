using System;
using System.Collections.Generic;
using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Grids;
using ATCG.Battle.Players;
using ATCG.Capacities;
using ATCG.Capacities.Data.Fire;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Capacities.Fire
{
    public partial struct HuntOfTheSoul : ICapacity<HuntOfTheSoulData>
    {
        // Valid default: tags the cell as CELL and every member on it as MEMBER.
        public void GetTargets(HuntOfTheSoulData data, BattleCellAspect battleCell, CapacityTargets output, IBattlePlayer castingPlayer)
        {
            output.Add(battleCell.EntityAddress, CapacityTags.CELL);
            foreach (var member in battleCell.GetMembers())
                output.Add(member.EntityAddress, CapacityTags.MEMBER);
        }

        public void GetHitPattern(HuntOfTheSoulData data, ref HexPatternBuilder builder, BattleGrid battleGrid, HexCoordinates castPoint, HexCoordinates casterOrigin)
        {
            builder.With(new PointsPattern(castPoint))
                .Without(casterOrigin);
        }

        // Step wired by [WithStep("Spawn")] on HuntOfTheSoulData.
        private partial void ExecuteSpawn(HuntOfTheSoulData data, CapacityStepContext ctx)
        {
            foreach (var cellAddress in ctx.Targets.WithTags(CapacityTags.CELL))
            {
                if (cellAddress.Is(out BattleCellAspect cellAspect))
                {
                    var spawnDeployable = new SpawnDeployableCommand(
                        ctx.BattlePhase.CurrentPlayer,
                        data.Deployable,
                        cellAspect,
                        ctx.Caster);
                    spawnDeployable.Run(ctx.BattlePhase);
                }
            }
        }
    }
}
