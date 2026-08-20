using System;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Players;

namespace ATCG.Battle.Commands.Players
{
    [Serializable]
    public sealed class PlayerCommandSignal : PlayerCommand<NoInfos>, ICommandSignal
    {
        public Guid Channel { get; private set; }

        public PlayerCommandSignal(IBattlePlayer battlePlayer, Guid channel, string source = DEFAULT_SOURCE) : base(battlePlayer, source)
        {
            Channel = channel;
        }

        protected override void Process(in CommandContext context)
        {
            
        }
    }
}