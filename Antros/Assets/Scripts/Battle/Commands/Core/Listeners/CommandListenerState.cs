using System;
using System.Collections.Generic;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Core.Players;
using Helteix.Tools;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Commands.Players
{
    public readonly struct CommandListenerState : IDisposable
    {
        private readonly AwaitableCompletionSource windUpSource;
        private readonly AwaitableCompletionSource followThroughSource;

        private readonly Dictionary<ICommandListener, bool> windUps;
        private readonly Dictionary<ICommandListener, bool> followThrough;

        public Awaitable WindUp => windUpSource.Awaitable;
        public Awaitable FollowThrough => followThroughSource.Awaitable;


        public CommandListenerState(IEnumerable<ICommandListener> players, float timeout = 5f)
        {
            windUpSource = new AwaitableCompletionSource();
            followThroughSource = new AwaitableCompletionSource();

            windUps = DictionaryPool<ICommandListener, bool>.Get();
            followThrough = DictionaryPool<ICommandListener, bool>.Get();

            foreach (var player in players)
            {
                windUps[player] = false;
                followThrough[player] = false;
            }
            if (windUps.Count == 0)
                windUpSource.TrySetResult();
            else
                WaitForWindup(timeout).ListenForExceptions();

            if (followThrough.Count == 0)
                followThroughSource.TrySetResult();
            else
                WaitForFollowThrough(timeout * 2f).ListenForExceptions();
        }

        private async Awaitable WaitForWindup(float time)
        {
            await Awaitable.WaitForSecondsAsync(time);
            if (windUpSource.TrySetResult())
                Debug.LogWarning($"CommandListenerState: Timeout reached for wind up after {time} seconds");
        }

        private async Awaitable WaitForFollowThrough(float time)
        {
            await Awaitable.WaitForSecondsAsync(time);
            if (followThroughSource.TrySetResult())
                Debug.LogWarning($"CommandListenerState: Timeout reached for follow through after {time} seconds");
        }

        public void CompleteWindUp(ICommandListener listener)
        {
            windUps[listener] = true;
            bool isDone = true;
            foreach (var value in windUps.Values)
                isDone &= value;

            if (isDone)
            {
                //Debug.Log("All players have completed wind up");
                windUpSource.TrySetResult();
            }
        }

        public void CompleteFollowThrough(ICommandListener listener)
        {
            followThrough[listener] = true;

            bool isDone = true;
            foreach (var value in followThrough.Values)
                isDone &= value;

            if (isDone)
            {
                followThroughSource.TrySetResult();
                //Debug.Log("All players have completed follow through");
            }
        }
        public void CompleteAll(ICommandListener listener)
        {
            CompleteWindUp(listener);
            CompleteFollowThrough(listener);
        }

        void IDisposable.Dispose()
        {
            if (windUps != null)
                DictionaryPool<ICommandListener, bool>.Release(windUps);
            if (followThrough != null)
                DictionaryPool<ICommandListener, bool>.Release(followThrough);
        }

    }
}