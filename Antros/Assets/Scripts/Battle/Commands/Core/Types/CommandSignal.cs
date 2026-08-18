using System;
using ATCG.Battle.Commands.Infos;

namespace ATCG.Battle.Commands
{
    [Serializable]
    public abstract class CommandSignal<T> : Command<T>, ICommandSignal where T : struct, ICommandInfos
    {
        protected CommandSignal(string source = DEFAULT_SOURCE) : base(source)
        {

        }

        protected sealed override void Process(in CommandContext context)
        {

        }
    }
}