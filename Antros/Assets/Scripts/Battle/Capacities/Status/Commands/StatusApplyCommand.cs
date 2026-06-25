using ATCG.Battle.Capacities.Mapping;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components.Implementations;
using ATCG.Battle.Entities.Components.Status;
using ATCG.Battle.Entities.Components.Status.Signals;
using ATCG.Capacities.Data.Status;
using Helteix.Tools.DataMapping;
using UnityEngine;

namespace ATCG.Battle.Commands.EntityCommands
{
    public class StatusApplyCommand : EntityCommand<StatusApplyCommand.Infos>
    {
        [SerializeField]
        private StatusData data;

        public struct Infos : ICommandInfos
        {
        }

        private readonly bool tickResult;

        public StatusApplyCommand(EntityAddress address, StatusData data) : base(address)
        {
            this.data = data;
        }

        protected override void Process(in CommandContext context)
        {
            if (Mapper.TryGet(data, out IStatusContainer container))
            {
                StatusContext statusContext = new StatusContext(context.battlePhase);
                container.Apply(data, Target.ToAddress(context.World), statusContext);
            }
        }
    }
}