using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Infos;
using ATCG.Capacities.Data.Status;
using Helteix.Tools.DataMapping;

namespace ATCG.Battle.CapacitySystem.Core.Status.Commands
{
    public class StatusTickAllCommand : Command<NoInfos>
    {
        private readonly StatusData data;

        public StatusTickAllCommand(StatusData data)
        {
            this.data = data;
        }

        protected override void Process(in CommandContext context)
        {
            if (Mapper.TryGet(data, out IStatusContainer container))
            {
                StatusContext statusContext = new StatusContext(context.battlePhase);
                //container.TickAll(data, statusContext);
            }
        }
    }
}