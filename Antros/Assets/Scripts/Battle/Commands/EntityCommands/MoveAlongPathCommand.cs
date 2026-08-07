using System.Collections.Generic;
using System.Linq;
using ATCG.Battle.Commands.Entities;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Entities;
using ATCG.HexGrids;

namespace ATCG.Battle.Commands.EntityCommands
{
    public class MoveAlongPathCommand : EntityCommand<MoveAlongPathCommand.Infos>
    {
        public struct Infos : ICommandInfos
        {
            public HexCoordinates[] path;
        }

        private readonly IEnumerable<HexCoordinates> path;
        private readonly int maxSteps;

        public MoveAlongPathCommand(EntityAddress address, IEnumerable<HexCoordinates> path, int maxSteps = int.MaxValue) : base(address)
        {
            this.path = path;
            this.maxSteps = maxSteps;
        }

        protected override void Process(in CommandContext context)
        {
            //var stepCount = -1; //Origin is in the path and we dont want to include it in the count
            foreach (HexCoordinates coord in path)
            {
                //if(stepCount >= maxSteps)
                //  break;
                
                var moveCommand = new MoveCommand(Target.ToAddress(context), coord);
                Inject(context, moveCommand);
                //stepCount++;
            }

            infos.path = path.ToArray();
        }
    }
}