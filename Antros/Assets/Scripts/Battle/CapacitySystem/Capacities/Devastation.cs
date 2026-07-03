using System.Collections.Generic;
using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Controllers;
using ATCG.Capacities;
using ATCG.Capacities.Fire;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;
using ATCG.Utilities;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Capacities
{
    public partial struct Devastation : ICapacity<DevastationData>
    {
        public HexPatternBuilder GetHitPattern(DevastationData data, BattleGrid battleGrid, HexCoordinates castPoint, HexCoordinates casterOrigin)
        {
            BattleIgnoreOriginPatternController hexPatternController = new(battleGrid, castPoint);
            HexPatternBuilder builder = new HexPatternBuilder(castPoint, hexPatternController)
                .With(new SpreadPattern(data.Range));

            return builder;
        }

        private partial void ExecuteExplosion(DevastationData data, CapacityStepContext ctx)
        {
            BattleGrid battleGrid = ctx.BattlePhase.BattleGrid;

            using HexPatternBuilder builder = GetHitPattern(data, battleGrid, ctx.CastPoint, ctx.capacityPhase.CasterOrigin);

            int damage = GameMaths.Round(data.Damage.Evaluate(ctx.effectiveness));
            foreach (BattleCellAspect cellAspect in builder.GetBattleCells(battleGrid))
            {
                foreach (ComponentRef<GridMemberComponent> member in cellAspect.GetMembers())
                {
                    if (!member.EntityAddress.HasComponent<HealthComponent>())
                        continue;

                    if (!ctx.IsAlly(member.EntityAddress))
                    {
                        DamageCommand damageCommand = new DamageCommand(damage, member.EntityAddress);
                        damageCommand.Run(ctx.BattlePhase);
                    }
                }
            }
        }
    }

}