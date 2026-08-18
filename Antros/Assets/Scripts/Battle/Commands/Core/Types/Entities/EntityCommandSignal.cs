using System;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Entities;

namespace ATCG.Battle.Commands.Entities
{
    [Serializable]
    public abstract class EntityCommandSignal<T> : EntityCommand<T> where T : struct, ICommandInfos
    {
        protected EntityCommandSignal(EntityAddress address, string source = DEFAULT_SOURCE) : base(address, source)
        {

        }

        protected sealed override void Process(in CommandContext context)
        {

        }
    }
}