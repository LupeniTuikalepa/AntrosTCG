using System;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Entities;
using UnityEngine;

namespace ATCG.Battle.Commands.Entities
{
    [Serializable]
    public sealed class EntityCommandSignal : EntityCommand<NoInfos>, ICommandSignal
    {
        public Guid Channel { get; private set; }
        
        public EntityCommandSignal(EntityAddress address, Guid channel, string source = DEFAULT_SOURCE) : base(address, source)
        {
            Channel = channel;
        }

        protected override void Process(in CommandContext context)
        {
        }

    }
}