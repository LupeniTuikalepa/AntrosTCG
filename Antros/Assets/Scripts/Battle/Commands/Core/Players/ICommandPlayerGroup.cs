using System;
using System.Threading.Tasks;
using Helteix.Tools;
using UnityEngine;

namespace ATCG.Battle.Commands.Core
{
    public interface ICommandPlayerGroup: IDisposable
    {
        IGameCommand Command { get; }
        Awaitable Initiate(GameCommandContext context);

        async Awaitable WaitBeforePlayingEmbed(IGameCommand embed) => await Task.CompletedTask;

        async Awaitable WaitAfterPlayingEmbed(IGameCommand embed) => await Task.CompletedTask;
    }
}