using ATCG.Battle.Commands;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Commands.Listeners;
using ATCG.Battle.Commands.Players;
using ATCG.Battle.Players;
using UnityEngine;

namespace ATCG.Battle.PassiveSystem.Core
{
    public abstract class Passive<TCommand> :
        PlayerCommand<NoInfos>,
        ICommandListener<TCommand>
        where TCommand : ICommand
    {
        protected Passive(IBattlePlayer battlePlayer) : base(battlePlayer)
        {
        }

        async Awaitable ICommandListener<TCommand>.Play(CommandListenerState state, CommandContext context, TCommand command)
        {
            await Awaitable.MainThreadAsync();
            if(CanInjectPassive(context, command))
                Inject(context, this);
        }

        protected abstract bool CanInjectPassive(CommandContext context, TCommand command);
    }
}