using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Controllers;
using ATCG.Capacities.Frost;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Arc;
using ATCG.HexGrids.Patterns.Building;
using ATCG.HexGrids.Utility;

namespace ATCG.Battle.CapacitySystem.Capacities.Frost
{
    public partial struct IceshardHammer : ICapacity<IceshardHammerData>
    {
        public HexPatternBuilder GetHitPattern(IceshardHammerData data, BattleGrid battleGrid, HexCoordinates castPoint,
            HexCoordinates casterOrigin)
        {
            BattleIgnoreOriginPatternController hexPatternController = new(battleGrid, castPoint);
            HexPatternBuilder builder = new HexPatternBuilder(castPoint, hexPatternController)
                .With(new PointsPattern(castPoint));

            return builder;
        }

        private partial void ExecuteDestruction(IceshardHammerData data, CapacityStepContext ctx)
        {
            BattleGrid battleGrid = ctx.BattlePhase.BattleGrid;

            using HexPatternBuilder builder = GetHitPattern(data, battleGrid, ctx.CastPoint, ctx.capacityPhase.CasterOrigin);

            foreach (BattleCellAspect cellAspect in builder.GetBattleCells(battleGrid))
            {
                foreach (var member in cellAspect.GetPhysicalMembers())
                {
                    var memberEntityAddress = member.EntityAddress;
                    if (memberEntityAddress.Is<DeployableAspect>(out var deployable))
                    {
                        var deployableData = deployable.DeployableEntityTag.data;
                        var direction = ctx.CasterOrigin.GetNormalizedDirection(ctx.CastPoint);
                        var propagation = member.GetValue().coordinates + direction;
                        
                        if (deployableData is not IceWallData)
                            continue;   
                        
                        var damageCommand = new DamageCommand(99, memberEntityAddress);
                        damageCommand.Run(ctx.BattlePhase);

                        battleGrid.TryGetBattleCell(propagation, out var cell);
                        foreach (var physicalMember in cell.GetPhysicalMembers())
                        {
                            var propagationDamageCommand = new DamageCommand(data.Damage, physicalMember.EntityAddress);
                            propagationDamageCommand.Run(ctx.BattlePhase);
                        }
                        
                    }
                    else if (memberEntityAddress.TryGetComponentRO<MovementComponent>(out _))
                    {
                        var direction = ctx.CasterOrigin.GetNormalizedDirection(cellAspect.Coordinate);
                        var destination = cellAspect.Coordinate + direction * data.PushbackMultiplier;
                        
                        var pushbackCommand = new PushbackCommand(memberEntityAddress, destination);
                        pushbackCommand.Run(ctx.BattlePhase);
                    }
                }
            }
        }
    }
}