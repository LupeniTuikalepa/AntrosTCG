using System;
using System.Threading.Tasks;
using Helteix.Tools;
using UnityEngine;

namespace ATCG.Battle.Commands.Core
{
    public interface ICommandPlayerContext: IDisposable
    {
        GameCommand Command { get; }
        Awaitable Initiate(GameCommandContext context);

        async Awaitable WaitBeforePlayingEmbed(GameCommand embed) => await Task.CompletedTask;

        async Awaitable WaitAfterPlayingEmbed(GameCommand embed) => await Task.CompletedTask;
    }
}