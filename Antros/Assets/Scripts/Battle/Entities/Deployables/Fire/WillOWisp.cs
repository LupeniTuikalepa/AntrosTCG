using ATCG.Battle.CapacitySystem.Core.Status.Commands;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Commands;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Queries;
using ATCG.Battle.Grids;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Capacities.Fire;
using ATCG.Enums;
using ATCG.HexGrids;
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
                    MoveToEnemy(context, command, aspect, data));
        }

        private static void MoveToEnemy(
            CommandContext context,
            EndTurnCommand command,
            DeployableAspect aspect,
            WillOWispData data)
        {
            var builder = new EntityQueryBuilder()
                .WithAllComponents<HealthComponent>()
                .WithAllComponents<BelongsToPlayerComponent>()
                    .Where(address => 
                        !address.IsAlly(aspect.BelongsToPlayerComponent.GetPlayer(context.battlePhase)));

            var minDistance = int.MaxValue;
            var destination = aspect.GridMemberComponent.coordinates;

            using (ListPool<HexCoordinates>.Get(out var path))
            {
                foreach (var address in context.World.Query(builder))
                {
                    if (!address.TryGetComponentRO<GridMemberComponent>(out var gridMember)) 
                        continue;
                    
                    if(!aspect.EntityAddress.Is<PathfindingAgentAspect>(out var agent))
                        continue;
                    
                    if(!HexPathfinder.TryBuildPath(
                           aspect.GridMemberComponent.coordinates,
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
                
                if (destination == aspect.GridMemberComponent.coordinates)
                    return;
                
                path.Remove(destination);
                var moveAlongPathCommand = new MoveAlongPathCommand(aspect.EntityAddress, path.ToArray(), data.MoveSpeed);
                command.Inject(context, moveAlongPathCommand);
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