using System;
using System.Collections.Generic;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Core.Players;
using Helteix.Tools;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Commands.Players
{
    public readonly struct CommandPlayerState : IDisposable
    {
        private readonly AwaitableCompletionSource windUpSource;
        private readonly AwaitableCompletionSource followThroughSource;

        private readonly Dictionary<ICommandPlayer, bool> windUps;
        private readonly Dictionary<ICommandPlayer, bool> followThrough;

        public Awaitable WindUp => windUpSource.Awaitable;
        public Awaitable FollowThrough => followThroughSource.Awaitable;


        public CommandPlayerState(IEnumerable<ICommandPlayer> players, float timeout = 5f)
        {
            windUpSource = new AwaitableCompletionSource();
            followThroughSource = new AwaitableCompletionSource();

            windUps = DictionaryPool<ICommandPlayer, bool>.Get();
            followThrough = DictionaryPool<ICommandPlayer, bool>.Get();

            foreach (var player in players)
            {
                windUps[player] = false;
                followThrough[player] = false;
            }
            if (windUps.Count == 0)
                windUpSource.TrySetResult();
            else
                WaitForWindup(timeout).FireAndForget();

            if (followThrough.Count == 0)
                followThroughSource.TrySetResult();
            else
                WaitForFollowThrough(timeout * 2f).FireAndForget();
        }

        private async Awaitable WaitForWindup(float time)
        {
            await Awaitable.WaitForSecondsAsync(time);
            if (windUpSource.TrySetResult())
                Debug.LogWarning($"CommandPlayerState: Timeout reached for wind up after {time} seconds");
        }

        private async Awaitable WaitForFollowThrough(float time)
        {
            await Awaitable.WaitForSecondsAsync(time);
            if (followThroughSource.TrySetResult())
                Debug.LogWarning($"CommandPlayerState: Timeout reached for follow through after {time} seconds");
        }

        public void CompleteWindUp(ICommandPlayer player)
        {
            windUps[player] = true;
            bool isDone = true;
            foreach (var value in windUps.Values)
                isDone &= value;

            if (isDone)
            {
                //Debug.Log("All players have completed wind up");
                windUpSource.TrySetResult();
            }
        }

        public void CompleteFollowThrough(ICommandPlayer player)
        {
            followThrough[player] = true;

            bool isDone = true;
            foreach (var value in followThrough.Values)
                isDone &= value;

            if (isDone)
            {
                followThroughSource.TrySetResult();
                //Debug.Log("All players have completed follow through");
            }
        }

        void IDisposable.Dispose()
        {
            if (windUps != null)
                DictionaryPool<ICommandPlayer, bool>.Release(windUps);
            if (followThrough != null)
                DictionaryPool<ICommandPlayer, bool>.Release(followThrough);
        }
    }
}