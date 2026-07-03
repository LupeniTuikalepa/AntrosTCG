using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Entities;
using ATCG.Capacities.Data.Status;

namespace ATCG.Battle.CapacitySystem.Core.Status.Signals
{
    public class StatusSignal : EntityCommand<StatusSignal.Infos>
    {
        public struct Infos : ICommandInfos
        {
            public int id;
            public StatusAction action;
            public StatusData data;
        }
        
        public StatusSignal(EntityAddress address, StatusAction action, StatusData data) : base(address)
        {
            infos.id = address.entity.id;
            infos.action = action;
            infos.data = data;
        }
        
        protected override void Process(in CommandContext context)
        {
        }
    }
}