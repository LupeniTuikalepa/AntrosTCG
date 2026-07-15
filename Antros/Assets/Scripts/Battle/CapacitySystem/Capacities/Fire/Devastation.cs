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
using ATCG.Capacities.Fire;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;
using ATCG.Utilities;

namespace ATCG.Battle.CapacitySystem.Capacities
{
    public partial struct Devastation : ICapacity<DevastationData>
    {
        public void GetTargets(DevastationData data, BattleCellAspect battleCell, List<EntityAddress> output)
        {
            output.Add(battleCell.EntityAddress);
            foreach (var member in battleCell.GetMembers())
            {
                if (!member.EntityAddress.HasComponent<HealthComponent>())
                    continue;

                output.Add(member.EntityAddress);
            }
        }

        public HexPatternBuilder GetHitPattern(DevastationData data, BattleGrid battleGrid, HexCoordinates castPoint, HexCoordinates casterOrigin)
        {
            BattleIgnoreOriginPatternController hexPatternController = new(battleGrid, castPoint);
            HexPatternBuilder builder = new HexPatternBuilder(castPoint, hexPatternController)
                .With(new SpreadPattern(data.Range));

            return builder;
        }


        private partial void ExecuteExplosion(DevastationData data, CapacityStepContext ctx)
        {
            int damage = GameMaths.Round(data.Damage.Evaluate(ctx.effectiveness));
            foreach (CapacityTarget target in ctx.Targets)
            {
                if (!target.address.HasComponent<HealthComponent>())
                    continue;

                if (!ctx.IsAlly(target.address))
                {
                    DamageCommand damageCommand = new DamageCommand(damage, target.address);
                    damageCommand.Run(ctx.BattlePhase);
                }
            }
        }
    }
}