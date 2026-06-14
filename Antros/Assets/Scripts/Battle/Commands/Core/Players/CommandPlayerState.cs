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

        private readonly Dictionary<object, bool> windUps;
        private readonly Dictionary<object, bool> followThrough;

        public Awaitable WindUp => windUpSource.Awaitable;
        public Awaitable FollowThrough => followThroughSource.Awaitable;

        public CommandPlayerState(IEnumerable<object> players, float timeout = 5f)
        {
            windUpSource = new AwaitableCompletionSource();
            followThroughSource = new AwaitableCompletionSource();

            windUps = DictionaryPool<object, bool>.Get();
            followThrough = DictionaryPool<object, bool>.Get();

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

        public void CompleteWindUp<T>(ICommandPlayer<T> player) where T : IGameCommand
        {
            windUps[player] = true;
            bool isDone = true;
            foreach (var value in windUps.Values)
                isDone &= value;

            if (isDone)
            {
                Debug.Log("All players have completed wind up");
                windUpSource.TrySetResult();
            }
        }

        public void CompleteFollowThrough<T>(ICommandPlayer<T> player) where T : IGameCommand
        {
            followThrough[player] = true;

            bool isDone = true;
            foreach (var value in followThrough.Values)
                isDone &= value;

            if (isDone)
            {
                followThroughSource.TrySetResult();
                Debug.Log("All players have completed follow through");
            }
        }

        void IDisposable.Dispose()
        {
            if (windUps != null)
                DictionaryPool<object, bool>.Release(windUps);
            if (followThrough != null)
                DictionaryPool<object, bool>.Release(followThrough);
        }
    }
}