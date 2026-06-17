using System.Collections;
using System.Collections.Generic;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Entities;
using ATCG.HexGrids;
using UnityEngine.Pool;

namespace ATCG.Battle.Commands.EntityCommands
{
    public class MovePathCommand : EntityCommand<NoInfos>
    {
        private readonly List<HexCoordinates> path;

        public MovePathCommand(EntityAddress address, IEnumerable<HexCoordinates> coordinates) : base(address)
        {
            path = ListPool<HexCoordinates>.Get();
            path.AddRange(coordinates);
        }

        protected override void Process(in CommandContext context)
        {
            foreach (var coordinate in path)
            {
                var moveCommand = new MoveCommand(Target.ToAddress(context), coordinate);
                Embed(context, moveCommand);
            }
        }

        protected override void Dispose(in CommandContext context)
        {
            base.Dispose(in context);
            ListPool<HexCoordinates>.Release(path);
        }
    }
}