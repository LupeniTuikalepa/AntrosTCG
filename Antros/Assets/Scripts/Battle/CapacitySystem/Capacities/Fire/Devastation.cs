using System;
using System.Collections.Generic;
using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Controllers;
using ATCG.Battle.Players;
using ATCG.Capacities;
using ATCG.Capacities.Fire;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;
using ATCG.Utilities;

namespace ATCG.Battle.CapacitySystem.Capacities
{
    public partial struct Devastation : ICapacity<DevastationData>
    {
        public void GetTargets(DevastationData data, BattleCellAspect battleCell, CapacityTargets output, IBattlePlayer castingPlayer)
        {
            output.Add(battleCell.EntityAddress, CapacityTags.CELL);
            foreach (var member in battleCell.GetMembers())
            {
                if (!member.EntityAddress.HasComponent<HealthComponent>())
                    continue;

                output.Add(member.EntityAddress, CapacityTags.MEMBER);
            }
        }

        public void GetHitPattern(DevastationData data, ref HexPatternBuilder builder, BattleGrid battleGrid, HexCoordinates castPoint, HexCoordinates casterOrigin)
        {
            builder .With(new SpreadPattern(data.Range));
        }


        private partial void ExecuteExplosion(DevastationData data, CapacityStepContext ctx)
        {
            int damage = GameMaths.Round(data.Damage.Evaluate(ctx.effectiveness));
            foreach (EntityAddress target in ctx.Targets.WithTags(CapacityTags.MEMBER))
            {
                if (ctx.IsAlly(target))
                    continue;

                DamageCommand damageCommand = new DamageCommand(damage, target);
                damageCommand.Run(ctx.BattlePhase);
            }
        }
    }
}