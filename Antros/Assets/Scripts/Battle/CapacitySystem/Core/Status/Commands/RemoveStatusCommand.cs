using System;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.Entities;
using ATCG.Battle.Entities;
using ATCG.Capacities.Data.Status;
using Helteix.Tools.DataMapping;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Core.Status.Commands
{
    [Serializable]
    public class RemoveStatusCommand : EntityCommand<StatusCommandInfos>
    {
        [field: SerializeField]
        public StatusData Data { get; private set; }

        public RemoveStatusCommand(EntityAddress address, StatusData data) : base(address)
        {
            this.Data = data;
        }

        protected override void Process(in CommandContext context)
        {
            infos.data = Data;
            if (Mapper.TryGet(Data, out IStatusContainer container))
            {
                StatusContext statusContext = new StatusContext(context.battlePhase);
                container.Remove(Data, Target.ToAddress(context.World), statusContext);
            }
        }
    }
}