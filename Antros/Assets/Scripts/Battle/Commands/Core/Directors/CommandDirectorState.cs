using System;
using System.Collections.Generic;
using Helteix.Tools;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Commands.Directors
{
    public readonly struct CommandDirectorState : IDisposable
    {
        private readonly AwaitableCompletionSource windUpSource;
        private readonly AwaitableCompletionSource followThroughSource;

        private readonly Dictionary<ICommandDirector, bool> windUps;
        private readonly Dictionary<ICommandDirector, bool> followThrough;

        public Awaitable WindUp => windUpSource.Awaitable;
        public Awaitable FollowThrough => followThroughSource.Awaitable;


        public CommandDirectorState(IEnumerable<ICommandDirector> players, float timeout = 5f)
        {
            windUpSource = new AwaitableCompletionSource();
            followThroughSource = new AwaitableCompletionSource();

            windUps = DictionaryPool<ICommandDirector, bool>.Get();
            followThrough = DictionaryPool<ICommandDirector, bool>.Get();

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

        public void CompleteWindUp(ICommandDirector director)
        {
            windUps[director] = true;
            bool isDone = true;
            foreach (var value in windUps.Values)
                isDone &= value;

            if (isDone)
            {
                //Debug.Log("All players have completed wind up");
                windUpSource.TrySetResult();
            }
        }

        public void CompleteFollowThrough(ICommandDirector director)
        {
            followThrough[director] = true;

            bool isDone = true;
            foreach (var value in followThrough.Values)
                isDone &= value;

            if (isDone)
            {
                followThroughSource.TrySetResult();
                //Debug.Log("All players have completed follow through");
            }
        }
        public void CompleteAll(ICommandDirector director)
        {
            CompleteWindUp(director);
            CompleteFollowThrough(director);
        }

        void IDisposable.Dispose()
        {
            if (windUps != null)
                DictionaryPool<ICommandDirector, bool>.Release(windUps);
            if (followThrough != null)
                DictionaryPool<ICommandDirector, bool>.Release(followThrough);
        }

    }
}