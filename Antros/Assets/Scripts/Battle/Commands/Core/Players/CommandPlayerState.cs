using System;
using System.Collections.Generic;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Core.Players;
using Helteix.Tools;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Commands.Players
{
    public readonly struct CommandPlayerState: IDisposable
    {
        private readonly AwaitableCompletionSource windUpSource;
        private readonly AwaitableCompletionSource followThroughSource;
        
        private readonly Dictionary<object, bool> windUps;
        private readonly Dictionary<object, bool> followThrough;

        public Awaitable WindUp => windUpSource.Awaitable;
        public Awaitable FollowThrough => followThroughSource.Awaitable;

        public CommandPlayerState(float maxWindUpTime = 5f, float maxFollowThroughTime = 5f)
        {
            windUpSource = new AwaitableCompletionSource();
            followThroughSource = new AwaitableCompletionSource();

            windUps = DictionaryPool<object, bool>.Get();
            followThrough = DictionaryPool<object, bool>.Get();
            
            WaitFor(maxWindUpTime, windUpSource).FireAndForget();
            WaitFor(maxFollowThroughTime, followThroughSource).FireAndForget();
        }

        private static async Awaitable WaitFor(float time, AwaitableCompletionSource source)
        {
            await Awaitable.WaitForSecondsAsync(time);
            source.TrySetResult();
        }
        public void CompleteWindUp<T>(ICommandPlayer<T> player) where T : IGameCommand
        {
            if(windUpSource.Awaitable.IsCompleted)
                return;
            
            windUps[player] = true;
            bool isDone = true;
            foreach (var value in windUps.Values)
                isDone &= value;

            if (isDone)
                windUpSource.TrySetResult();
        }

        public void CompleteFollowThrough<T>(ICommandPlayer<T> player) where T : IGameCommand
        {
            followThrough[player] = true;
            
            bool isDone = true;
            foreach (var value in followThrough.Values)
                isDone &= value;

            if (isDone)
                followThroughSource.TrySetResult();
        }

        void IDisposable.Dispose()
        {
            DictionaryPool<object, bool>.Release(windUps);
            DictionaryPool<object, bool>.Release(followThrough);
        }
    }
}