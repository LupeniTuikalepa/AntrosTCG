using System;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.Entities;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Entities;
using ATCG.Capacities.Data.Status;
using Helteix.Tools.DataMapping;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Core.Status.Commands
{

    [Serializable]
    public struct StatusCommandInfos : ICommandInfos
    {
        public StatusData data;

        public StatusCommandInfos(StatusData data)
        {
            this.data = data;
        }
    }

    [Serializable]
    public class StatusApplyCommand : EntityCommand<StatusCommandInfos>
    {
        [field: SerializeField]
        public StatusData Data { get; private set; }

        private readonly bool tickResult;

        public StatusApplyCommand(EntityAddress address, StatusData data) : base(address)
        {
            this.Data = data;
        }

        protected override void Process(in CommandContext context)
        {
            infos.data = Data;
            if (Mapper.TryGet(Data, out IStatusContainer container))
            {
                StatusContext statusContext = new StatusContext(context.battlePhase);
                container.Apply(Data, Target.ToAddress(context.World), statusContext);
            }
        }
    }
}