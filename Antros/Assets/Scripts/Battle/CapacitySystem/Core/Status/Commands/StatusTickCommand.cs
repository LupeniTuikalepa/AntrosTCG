using ATCG.Battle.CapacitySystem.Status;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Entities;
using ATCG.Capacities.Data.Status;
using Helteix.Tools.DataMapping;

namespace ATCG.Battle.CapacitySystem.Core.Status.Commands
{
    public class StatusTickCommand : EntityCommand<NoInfos>
    {
        private readonly StatusData data;

        public StatusTickCommand(EntityAddress address, StatusData data) : base(address)
        {
            this.data = data;
        }

        protected override void Process(in CommandContext context)
        {
            if (Mapper.TryGet(data, out IStatusContainer container))
            {
                StatusContext statusContext = new StatusContext(context.battlePhase);
                container.Tick(data, Target.ToAddress(context.World), statusContext);
            }
        }
    }
}