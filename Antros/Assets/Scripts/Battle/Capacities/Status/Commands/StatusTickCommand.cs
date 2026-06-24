using System.Collections.Generic;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Entities;

namespace ATCG.Battle.Commands.EntityCommands
{
    public class StatusTickCommand : EntityCommand<NoInfos>
    {
        public StatusTickCommand(EntityAddress address) : base(address)
        {
        }

        protected override void Process(in CommandContext context)
        {
            StatusEffectCommand effectCommand = new StatusEffectCommand(TargetEntityAddress(context.World));
        }
    }
}