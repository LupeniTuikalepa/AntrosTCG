using ATCG.Battle.Commands;
using ATCG.Battle.Commands.Entities;
using ATCG.Battle.Entities;
using ATCG.Capacities.Data.Status;
using Helteix.Tools.DataMapping;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Core.Status.Commands
{
    public class StatusTickCommand : EntityCommand<StatusCommandInfos>
    {
        [field: SerializeField]
        public StatusData Data { get; private set; }

        public StatusTickCommand(EntityAddress address, StatusData data) : base(address)
        {
            this.Data = data;
        }

        protected override void Process(in CommandContext context)
        {
            infos.data = Data;
            if (Mapper.TryGet(Data, out IStatusContainer container))
            {
                StatusContext statusContext = new StatusContext(context.battlePhase);
                EntityAddress target = Target.ToAddress(context.World);

                container.Tick(Data, target, statusContext);
            }
        }
    }
}