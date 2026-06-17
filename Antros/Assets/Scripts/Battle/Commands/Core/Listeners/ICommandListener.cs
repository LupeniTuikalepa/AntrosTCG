using ATCG.Battle.Commands.Players;
using UnityEngine;

namespace ATCG.Battle.Commands.Core.Players
{
    public interface ICommandListener { }
    public interface ICommandListener<in T> : ICommandListener where T : ICommand
    {
        bool CanPlay(T command) => true;

        Awaitable Play(CommandListenerState state, CommandContext context, T command);
        void OnBegin(in CommandListenerState state, CommandContext context, T command) { }
        void OnHit(in CommandListenerState state, CommandContext context, T command) { }
        void OnEnd(in CommandListenerState state, CommandContext context, T command) { }
    }
}