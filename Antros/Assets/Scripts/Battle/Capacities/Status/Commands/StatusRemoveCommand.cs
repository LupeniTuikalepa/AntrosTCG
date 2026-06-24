using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Entities;

namespace ATCG.Battle.Commands.EntityCommands
{
    public class StatusRemoveCommand : EntityCommand<NoInfos>
    {
        public StatusRemoveCommand(EntityAddress address) : base(address)
        {
        }

        protected override void Process(in CommandContext context)
        {
        }
    }
}