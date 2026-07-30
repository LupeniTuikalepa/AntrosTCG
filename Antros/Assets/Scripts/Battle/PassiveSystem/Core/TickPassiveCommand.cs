using ATCG.Battle.Commands;
using ATCG.Battle.Commands.Entities;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Entities;
using Helteix.Tools.DataMapping;

namespace ATCG.Battle.PassiveSystem.Core
{
    public class TickPassiveCommand : EntityCommand<PassiveInfos>
    {
        private readonly PassiveContext passiveContext;

        public TickPassiveCommand(EntityAddress address, PassiveContext passiveContext) : base(address)
        {
            this.passiveContext = passiveContext;
        }

        protected override void Process(in CommandContext context)
        {
            var data = passiveContext.data;
            
            infos = new PassiveInfos(data);

            if(data.TryGet(out IPassiveContainer container))
                container.Tick(data, passiveContext);
        }
    }
}