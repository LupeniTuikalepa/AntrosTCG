using UnityEngine;

namespace ATCG.Battle.Commands.Core.Players
{
    public interface ICommandPlayer<in T> where T : IGameCommand
    {
        bool CanPlay(T command) => true;

        Awaitable Play(CommandContext context, T command);
        void OnBegin(CommandContext context, T command);
        void OnHit(CommandContext context, T command);
        void OnEnd(CommandContext context, T command);
    }
}