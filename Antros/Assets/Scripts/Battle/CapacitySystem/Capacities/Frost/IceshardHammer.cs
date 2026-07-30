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
using ATCG.Capacities.Frost;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;
using ATCG.HexGrids.Utility;

namespace ATCG.Battle.CapacitySystem.Capacities.Frost
{
    public partial struct IceshardHammer : ICapacity<IceshardHammerData>
    {
        public void GetHitPattern(IceshardHammerData data, ref HexPatternBuilder builder, BattleGrid battleGrid, HexCoordinates castPoint, HexCoordinates casterOrigin)
        {
            builder.With(new PointsPattern(castPoint));
        }

        public void GetTargets(IceshardHammerData data, BattleCellAspect battleCell, CapacityTargets output, IBattlePlayer castingPlayer)
        {
            output.Add(battleCell.EntityAddress, CapacityTags.CELL);
        }

        private partial void ExecuteDestruction(IceshardHammerData data, CapacityStepContext ctx)
        {
            foreach (EntityAddress memberEntityAddress in ctx.Targets.WithTags(CapacityTags.MEMBER))
            {
                if(!memberEntityAddress.Is<GridMemberAspect>(out var gridMemberAspect))
                    continue;

                if (memberEntityAddress.Is<DeployableAspect>(out var deployable))
                {
                    var deployableData = deployable.DeployableEntityTag.data;
                    var direction = ctx.CasterOrigin.GetNormalizedDirection(ctx.CastPoint);
                    var shardDestination = deployable.Coordinates + direction;

                    if (deployableData is not IceWallData)
                        continue;

                    var damageCommand = new DeathCommand(memberEntityAddress);
                    damageCommand.Run(ctx.BattlePhase);

                    ctx.BattleGrid.TryGetBattleCell(shardDestination, out var cell);
                    foreach (var physicalMember in cell.GetPhysicalMembers())
                    {
                        var propagationDamageCommand = new DamageCommand(data.Damage, physicalMember.EntityAddress);
                        propagationDamageCommand.Run(ctx.BattlePhase);
                    }
                }
                else if (memberEntityAddress.HasComponent<MovementComponent>())
                {
                    var direction = ctx.CasterOrigin.GetNormalizedDirection(gridMemberAspect.Coordinates);
                    var destination = gridMemberAspect.Coordinates + direction * data.PushbackMultiplier;

                    var pushbackCommand = new PushbackCommand(memberEntityAddress, destination);
                    pushbackCommand.Run(ctx.BattlePhase);
                }
            }
        }
    }
}