using System;
using ATCG.Battle.Commands.Infos;
using ATCG.Databases;

namespace ATCG.Battle.Commands
{
    public abstract class CommandSignal : Command<NoInfos>, ICommandSignal
    {
        public Guid Channel { get; private set; }

        public CommandSignal(Guid channel)
        {
            Channel = channel;
        }
        
        protected override void Process(in CommandContext context)
        {
        }

    }
}