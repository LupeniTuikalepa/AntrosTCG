using UnityEngine;

namespace ATCG.Battle.Commands.Core.Players
{
    public interface ICommandPlayer<in T> where T : IGameCommand
    {
        Awaitable Play(T command);
        bool CanPlay(T command) => true;
    }
}