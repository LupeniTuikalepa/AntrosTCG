using ATCG.Battle.CapacitySystem.Core.Status.Commands;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.Entities;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Commands;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Queries;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Controllers;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Capacities.Fire;
using ATCG.Enums;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;
using ATCG.HexGrids.Utility;
using Unity.Scripting.LifecycleManagement;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Entities.Deployables.Fire
{
    public partial struct WillOWisp : IDeployable<WillOWispData>
    {
        public void SetupEntity(WillOWispData data, DeployableAspect aspect)
        {
            aspect.EntityAddress.AddOrSetComponent(new HealthComponent(data.Health));
            aspect.EntityAddress.AddOrSetComponent(new PathfindingAgentComponent());

            aspect.EntityAddress.ListenForEntityCommand<MoveCommand>(
                (in CommandContext context, in MoveCommand command) =>
                    DropFlame(context, command, aspect, data));

            aspect.EntityAddress.ListenForPlayerCommand<EndTurnCommand>(
                false,
                (in CommandContext context, in EndTurnCommand command) =>
                    MoveAndAttack(context, command, aspect, data)
            );
        }

        private static void MoveAndAttack(
            CommandContext context,
            EndTurnCommand command,
            DeployableAspect aspect,
            WillOWispData data)
        {
            using (CommandManager.BeginGroup("WillOWisp Move And Attack"))
            {
                var origin = aspect.GridMemberComponent.coordinates;
                var battleGrid = context.Grid;

                var entityQueryBuilder = new EntityQueryBuilder()
                    .WithAllComponents<HealthComponent>()
                    .WithAllComponents<BelongsToPlayerComponent>()
                    .Where(address =>
                        !address.IsAlly(aspect.BelongsToPlayerComponent.GetPlayer(context.battlePhase)));

                var minDistance = int.MaxValue;
                var destination = origin;

                using (ListPool<HexCoordinates>.Get(out var path))
                {
                    foreach (var address in context.World.Query(entityQueryBuilder))
                    {
                        if (!address.TryGetComponentRO<GridMemberComponent>(out var gridMember))
                            continue;

                        if (!aspect.EntityAddress.Is<PathfindingAgentAspect>(out var agent))
                            continue;

                        if (!HexPathfinder.TryBuildPath(
                                origin,
                                gridMember.coordinates,
                                agent,
                                path,
                                data.MoveSpeed))
                            continue;

                        if (minDistance < path.Count)
                            continue;

                        minDistance = path.Count;
                        destination = gridMember.coordinates;
                    }

                    if (destination == origin)
                        return;

                    path.Remove(destination);
                    var moveAlongPathCommand = new MoveAlongPathCommand(aspect.EntityAddress, path.ToArray(), data.MoveSpeed);
                    command.Inject(context, moveAlongPathCommand);
                }

                var entitySignal = new EntityCommandSignal(aspect.EntityAddress, data.ID);
                command.Inject(context, entitySignal);

                var hexPatternBuilder = new HexPatternBuilder(
                        origin, new BattleIgnoreOriginPatternController(battleGrid, origin))
                        .With(new SpreadPatternData(data.AttackRange));

                foreach (var hexCell in hexPatternBuilder.GetCells(battleGrid))
                {
                    if (!battleGrid.TryGetBattleCell(hexCell.coordinates, out var cell))
                        continue;

                    foreach (var member in cell.GetMembers())
                    {
                        var memberAddress = member.EntityAddress;
                        if(memberAddress.IsAlly(aspect.BelongsToPlayerComponent.GetPlayer(context.battlePhase)))
                            continue;
                        
                        var damageCommand = new DamageCommand(data.Strength, memberAddress);
                        command.Inject(context, damageCommand);
                    }
                }
            }
        }

        private static void DropFlame(
            in CommandContext context,
            in MoveCommand command,
            DeployableAspect aspect,
            WillOWispData data)
        {
            var coord = aspect.GridMemberComponent.coordinates;

            if (!context.Grid.TryGetBattleCell(coord, out var cell))
                return;

            var applyStatusCommand = new ApplyStatusCommand(cell.EntityAddress, data.Status);
            command.Inject(context, applyStatusCommand);
        }
    }
}