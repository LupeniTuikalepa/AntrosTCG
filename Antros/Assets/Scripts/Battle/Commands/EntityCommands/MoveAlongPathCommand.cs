using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ATCG.Battle.Commands.Entities;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Grids;
using ATCG.HexGrids;
using UnityEngine.Pool;

namespace ATCG.Battle.Commands.EntityCommands
{
    public class MoveAlongPathCommand : EntityCommand<MoveAlongPathCommand.Infos>
    {
        public struct Infos : ICommandInfos
        {
            public HexCoordinates[] path;
        }

        private readonly IEnumerable<HexCoordinates> path;

        public MoveAlongPathCommand(EntityAddress address, IEnumerable<HexCoordinates> path) : base(address)
        {
            this.path = path;
        }

        protected override void Process(in CommandContext context)
        {
            foreach (HexCoordinates coord in path)
            {
                var moveCommand = new MoveCommand(Target.ToAddress(context), coord);
                Inject(context, moveCommand);
            }

            infos.path = path.ToArray();
        }
    }
}