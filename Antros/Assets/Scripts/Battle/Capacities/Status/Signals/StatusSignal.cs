using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Infos;

namespace ATCG.Battle.Entities.Components.Status.Signals
{
    public class StatusSignal : Command<StatusSignal.Infos>
    {
        public struct Infos : ICommandInfos
        {
            public int id;
            public StatusAction action;
        }
        
        public StatusSignal(int id, StatusAction action)
        {
            infos.id = id;
            infos.action = action;
        }
        
        protected override void Process(in CommandContext context)
        {
        }
    }
}