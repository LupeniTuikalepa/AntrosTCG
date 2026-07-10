using ATCG.Battle.Commands;
using ATCG.Battle.Commands.Entities;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Entities;
using ATCG.Capacities.Data.Status;
using Helteix.Tools.DataMapping;

namespace ATCG.Battle.CapacitySystem.Core.Status.Commands
{
    public class StatusRemoveCommand : EntityCommand<NoInfos>
    {
        private readonly StatusData data;

        public StatusRemoveCommand(EntityAddress address, StatusData data) : base(address)
        {
            this.data = data;
        }

        protected override void Process(in CommandContext context)
        {
            if (Mapper.TryGet(data, out IStatusContainer container))
            {
                StatusContext statusContext = new StatusContext(context.battlePhase);
                container.Remove(data, Target.ToAddress(context.World), statusContext);
            }
        }
    }
}