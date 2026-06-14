using ATCG.Battle.Commands.Players;
using UnityEngine;

namespace ATCG.Battle.Commands.Core.Players
{
    public interface ICommandPlayer<in T> where T : IGameCommand
    {
        bool CanPlay(T command) => true;

        Awaitable Play(CommandPlayerState state, CommandContext context, T command);
        void OnBegin(in CommandPlayerState state, CommandContext context, T command) { }
        void OnHit(in CommandPlayerState state, CommandContext context, T command) { }
        void OnEnd(in CommandPlayerState state, CommandContext context, T command) { }
    }
}