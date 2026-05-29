using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace Helteix.Tools
{
    public static class Awaitables
    {
        public static async Awaitable WhenAny(params Awaitable[] awaitables)
        {
            using (ListPool<Awaitable>.Get(out var list))
            {
                list.AddRange(awaitables);
                await list.WhenAny();
            }
        }

        public static async Awaitable WhenAny(this List<Awaitable> awaitables)
        {
            while (true)
            {
                foreach (Awaitable awaitable in awaitables)
                {
                    if (awaitable.IsCompleted)
                        return;
                }

                await Awaitable.NextFrameAsync();
            }
        }
        public static async Awaitable WhenAll(params Awaitable[] awaitables)
        {
            using (ListPool<Awaitable>.Get(out var list))
            {
                list.AddRange(awaitables);
                await list.WhenAll();
            }
        }

        public static async Awaitable WhenAll(this List<Awaitable> awaitables)
        {
            while (true)
            {
                bool valid = true;
                foreach (var awaitable in awaitables)
                {
                    if (!awaitable.IsCompleted)
                    {
                        valid = false;
                        break;
                    }
                }

                if(valid)
                    return;

                await Awaitable.NextFrameAsync();
            }
        }

        public static async Awaitable WaitUntil(Func<bool> condition)
        {
            while (!condition())
                await Awaitable.NextFrameAsync();
        }

        public static T GetResult<T>(this AwaitableCompletionSource<T> source)
        {
            if(source == null)
                return default;

            if(source.Awaitable == null)
                return default;

            Awaitable<T>.Awaiter awaiter = source.Awaitable.GetAwaiter();
            return awaiter.GetResult();
        }

    }
}