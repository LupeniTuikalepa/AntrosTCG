using System.Collections.Generic;
using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Grids;
using ATCG.Capacities;
using ATCG.Capacities.Fire;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;
using ATCG.Utilities;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Capacities
{
    public partial struct Devastation : ICapacity<DevastationData>
    {
        public IEnumerable<ICapacityStep> Run(DevastationData data, CastCapacityPhase phase)
        {
            //Explosion
            yield return new CapacityStep<DevastationData>(data, ApplyExplosion, DevastationData.Explosion);
        }

        private static void ApplyExplosion(DevastationData data, CapacityStepContext ctx)
        {
            BattleGrid battleGrid = ctx.BattlePhase.BattleGrid;
            BattlePatternController hexPatternController = new BattlePatternController(battleGrid);
            using HexPatternBuilder builder = new HexPatternBuilder(ctx.CastPoint, hexPatternController)
                .With(new SpreadPattern(data.Range));

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