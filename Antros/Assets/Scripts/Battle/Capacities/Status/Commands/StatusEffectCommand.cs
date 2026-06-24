using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components.Implementations;
using ATCG.Battle.Entities.Components.Status;

namespace ATCG.Battle.Commands.EntityCommands
{
    public class StatusEffectCommand : EntityCommand<StatusEffectCommand.Infos>
    {
        public struct Infos : ICommandInfos
        {
        }

        private readonly bool tickResult;

        public StatusEffectCommand(EntityAddress address) : base(address)
        {
        }

        protected override void Process(in CommandContext context)
        {
            //TODO a absolument elever
            TargetEntityAddress(context.World).ApplyStatus(new PoisonStatus(1), new StatusDurationController<PoisonStatus>(3));
        }
    }
}