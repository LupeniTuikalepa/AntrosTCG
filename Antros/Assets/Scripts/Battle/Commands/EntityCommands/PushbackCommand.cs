using System.Collections.Generic;
using System.Linq;
using ATCG.Battle.Commands.Entities;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Grids;
using ATCG.HexGrids;
using ATCG.HexGrids.Utility;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Commands.EntityCommands
{
    public class PushbackCommand : EntityCommand<PushbackCommand.Infos>
    {
        public struct Infos : ICommandInfos
        {
            public IEnumerable<MovementCoord> Path { get; private set; }

            public Infos(IEnumerable<MovementCoord> path)
            {
                Path = path;
            }
        }

        private readonly HexCoordinates direction;
        private readonly int strengthMultiplier;

        public PushbackCommand(EntityAddress address, HexCoordinates direction, int strengthMultiplier) : base(address)
        {
            this.direction = direction;
            this.strengthMultiplier = strengthMultiplier;
        }

        protected override void Process(in CommandContext context)
        {
            using (ListPool<MovementCoord>.Get(out var path))
            {
                if (!Target.TryGetComponent<GridMemberComponent>(context.World, out var targetGridMemberComponentRef))
                    return;

                if (!Target.ToAddress(context.World).Is<PathfindingAgentAspect>(out var agent))
                    return;

                ref GridMemberComponent component = ref targetGridMemberComponentRef.GetValue();

                var from = component.coordinates;
                var destination = from + direction;
                var collisionCord = HexCoordinates.None;
                
                for (int i = 0; i < strengthMultiplier; i++)
                {
                    var redirectPath =
                        HexPathfinder.ResolveRedirect(agent, context.Grid, from, destination, AgentMovementType.Push);

                    var movementCoords = redirectPath as MovementCoord[] ?? redirectPath.ToArray();
                    if (movementCoords.Length > 1)
                    {
                        path.AddRange(movementCoords);
                        break;
                    }

                    var dest = movementCoords.Last();
                    path.Add(dest);
                    
                    var nextDirection = from.GetNormalizedDirection(dest.destination).NearestCardinal();
                    from = dest.destination;
                    destination = from + nextDirection;
                }

                foreach (var movementCoord in path)
                {
                    var moveCommand = 
                        new MoveCommand(TargetEntityAddress(context.World), movementCoord.destination, movementCoord.movementType);
                    Inject(context, moveCommand);
                    
                }
                
                if (collisionCord.IsValid 
                    && context.Grid.TryGetBattleCell(collisionCord, out var collisionCell))
                {
                    var pushbackTarget = TargetEntityAddress(context.World);

                    foreach (var member in collisionCell.GetPhysicalMembers())
                    {
                        var impactDamageCommand = 
                            new ImpactDamageCommand(pushbackTarget, member.EntityAddress);
                        impactDamageCommand.Run(context.battlePhase);
                    }
                }

                infos = new Infos(path.ToArray());
            }
        }
    }
}