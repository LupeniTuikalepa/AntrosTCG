using ATCG.Battle.Commands;
using ATCG.Battle.Commands.Entities;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Entities;
using Helteix.Tools.DataMapping;

namespace ATCG.Battle.PassiveSystem.Core
{
    public class RemovePassiveCommand : EntityCommand<NoInfos>
    {
        private readonly PassiveContext passiveContext;

        public RemovePassiveCommand(EntityAddress address, PassiveContext passiveContext) : base(address)
        {
            this.passiveContext = passiveContext;
        }

        protected override void Process(in CommandContext context)
        {
            var data = passiveContext.data;
            if (data.TryGet(out IPassiveContainer container))
            {
                container.Remove(data, passiveContext);
            }
        }
    }
}