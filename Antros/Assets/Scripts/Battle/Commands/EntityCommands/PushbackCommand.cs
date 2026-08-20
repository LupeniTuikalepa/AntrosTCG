using System.Collections.Generic;
using ATCG.Battle.Commands.Entities;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Grids;
using ATCG.HexGrids;
using UnityEngine.Pool;

namespace ATCG.Battle.Commands.EntityCommands
{
    public class PushbackCommand : EntityCommand<PushbackCommand.Infos>
    {
        public struct Infos : ICommandInfos
        {
            public IEnumerable<HexCoordinates> Path { get; private set; }

            public Infos(IEnumerable<HexCoordinates> path)
            {
                Path = path;
            }
        }

        private readonly HexCoordinates destination;

        public PushbackCommand(EntityAddress address, HexCoordinates destination) : base(address)
        {
            this.destination = destination;
        }

        protected override void Process(in CommandContext context)
        {
            using (ListPool<HexCoordinates>.Get(out var path))
            {
                if (!Target.TryGetComponent<GridMemberComponent>(context.World, out var targetGridMemberComponentRef))
                    return;

                if (!Target.ToAddress(context.World).Is<PathfindingAgentAspect>(out var agent))
                    return;

                ref GridMemberComponent component = ref targetGridMemberComponentRef.GetValue();

                var from = component.coordinates;

                //TODO faire en sorte de prendre en compte toutes les cases si on a une strength de 2 ou plus
                var redirectDestination =
                    HexPathfinder.ResolveRedirect(agent, context.Grid, from, destination, path);
                
                if(!context.Grid.TryGetBattleCell(redirectDestination, out var redirectDestinationCell))
                    return;

                path.Remove(redirectDestination);
                
                var moveCommand = new MoveAlongPathCommand(TargetEntityAddress(context.World), path);
                Inject(context, moveCommand);
                
                foreach (var member in redirectDestinationCell.GetMembers())
                {
                    var impactDamageCommand = 
                        new ImpactDamageCommand(TargetEntityAddress(context.World), member.EntityAddress);
                    impactDamageCommand.Run(context.battlePhase);
                }
                
                infos = new Infos(path.ToArray());
            }
        }
    }
}