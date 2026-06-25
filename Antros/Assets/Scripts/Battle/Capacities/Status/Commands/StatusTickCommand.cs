using System.Collections.Generic;
using ATCG.Battle.Capacities.Mapping;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components.Status;
using ATCG.Capacities.Data.Status;
using Helteix.Tools.DataMapping;
using UnityEngine;

namespace ATCG.Battle.Commands.EntityCommands
{
    public class StatusTickCommand : EntityCommand<NoInfos>
    {
        private readonly StatusData data;
        private readonly bool tickAll;

        public StatusTickCommand(EntityAddress address, StatusData data, bool tickAll = false) : base(address)
        {
            this.data = data;
            this.tickAll = tickAll;
        }

        protected override void Process(in CommandContext context)
        {
            Debug.Log($"Battle phase: {context.battlePhase}");

            if (Mapper.TryGet(data, out IStatusContainer container))
            {
                StatusContext statusContext = new StatusContext(context.battlePhase);
                if (tickAll)
                    container.TickAll(data, statusContext);
                else
                    container.Tick(data, Target.ToAddress(context.World), statusContext);
            }
        }
    }
}