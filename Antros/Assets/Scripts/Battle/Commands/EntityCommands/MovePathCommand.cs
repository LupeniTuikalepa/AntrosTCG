using System.Collections;
using System.Collections.Generic;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Grids;
using ATCG.HexGrids;
using UnityEngine.Pool;

namespace ATCG.Battle.Commands.EntityCommands
{
    public class MovePathCommand : EntityCommand<MovePathCommand.Infos>
    {
        public struct Infos : ICommandInfos
        {
            public HexCoordinates[] path;
        }
        
        private readonly HexCoordinates to;

        public MovePathCommand(EntityAddress address, HexCoordinates to) : base(address)
        {
            this.to = to;
        }

        protected override void Process(in CommandContext context)
        {
            var address = TargetEntityAddress(context.World);
            if (!address.TryGetComponentRO(out GridMemberComponent component))
                return;
            
            using var hexPathfinder = new HexPathfinder(10000);
            using (ListPool<HexCoordinates>.Get(out var path))
            {
                hexPathfinder.FindPath(component.coordinates, to, path, context.Grid);
                infos.path = path.ToArray();
                
                foreach (var coordinate in path)
                {
                    var moveCommand = new MoveCommand(Target.ToAddress(context), coordinate);
                    Embed(context, moveCommand);
                }
            }
        }
    }
}