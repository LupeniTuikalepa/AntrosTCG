using System;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Players;

namespace ATCG.Battle.Commands.Players
{
    [Serializable]
    public abstract class PlayerCommandSignal<T> : PlayerCommand<T>, ICommandSignal where T : struct, ICommandInfos
    {
        protected PlayerCommandSignal(IBattlePlayer battlePlayer, string source = DEFAULT_SOURCE) : base(battlePlayer, source)
        {

        }

        protected sealed override void Process(in CommandContext context)
        {

        }
    }
}