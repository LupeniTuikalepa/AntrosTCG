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
                if (!Target.TryGetComponent<GridMemberComponent>(context.World, out var gridEntityComponentRef))
                    return;
                
                if(!Target.ToAddress(context.World).Is<PathfindingAgentAspect>(out var agent))
                    return;
                
                ref GridMemberComponent component = ref gridEntityComponentRef.GetValue();

                var redirectDestination =
                    HexPathfinder.ResolveRedirect(agent, context.Grid, component.coordinates, destination, path);

                component.coordinates = redirectDestination;

                infos = new Infos(path.ToArray());
            }
        }
    }
}