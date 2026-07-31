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

namespace ATCG.Battle.CapacitySystem.Capacities.Fire
{
    public partial struct HuntOfTheSoul : ICapacity<HuntOfTheSoulData>
    {
        public void GetHitPattern(HuntOfTheSoulData data, ref HexPatternBuilder builder, BattleGrid battleGrid, HexCoordinates castPoint, HexCoordinates casterOrigin)
        {
            builder.With(new PointsPattern(castPoint))
                .Without(casterOrigin);
        }

        // Valid default: tags the cell as Cell and every member on it as Member.
        public void GetTargets(HuntOfTheSoulData data, BattleCellAspect battleCell, CapacityTargets output,
            IBattlePlayer castingPlayer)
        {
            output.Add(battleCell.EntityAddress, CapacityTags.CELL);
            foreach (var member in battleCell.GetMembers())
                output.Add(member.EntityAddress, CapacityTags.MEMBER);
        }

        // Step wired by [WithStep("Summon")] on HuntOfTheDamned1Data.
        private partial void ExecuteSummon(HuntOfTheSoulData data, CapacityStepContext ctx)
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
