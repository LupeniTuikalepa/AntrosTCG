using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace Helteix.Tools
{
    public static class Awaitables
    {
        /// <summary>
        /// Lance un Awaitable en fire-and-forget et catche les exceptions qui surviennent
        /// après le premier point de suspension (await). Les exceptions levées dans la portion
        /// synchrone (avant le premier await) se propagent à l'appelant et sont loggées par Unity.
        /// </summary>
        /// <param name="awaitable">L'Awaitable déjà démarré à attendre.</param>
        /// <param name="onError">Handler d'erreur optionnel. Si null, utilise Debug.LogException.</param>
        public static async void FireAndForget(this Awaitable awaitable, Action<Exception> onError = null)
        {
            try
            {
                await awaitable;
            }
            catch (Exception e)
            {
                if (onError != null)
                    onError(e);
                else
                    Debug.LogException(e);
            }
        }

        /// <summary>
        /// Lance un Awaitable<T> en fire-and-forget et catche les exceptions qui surviennent
        /// après le premier point de suspension (await). Le résultat est ignoré. Les exceptions
        /// levées dans la portion synchrone (avant le premier await) se propagent à l'appelant
        /// et sont loggées par Unity.
        /// </summary>
        /// <param name="awaitable">L'Awaitable déjà démarré à attendre.</param>
        /// <param name="onError">Handler d'erreur optionnel. Si null, utilise Debug.LogException.</param>
        public static async void FireAndForget<T>(this Awaitable<T> awaitable, Action<Exception> onError = null)
        {
            try
            {
                await awaitable;
            }
            catch (Exception e)
            {
                if (onError != null)
                    onError(e);
                else
                    Debug.LogException(e);
            }
        }

        private static async Awaitable RunAsync<T>(this Func<Awaitable<T>> asyncMethod)
        {
            try
            {
                await asyncMethod();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static async Awaitable RunAsync(this Func<Awaitable> asyncMethod)
        {
            try
            {
                await asyncMethod();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

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

                if (valid)
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
            if (source == null)
                return default;

            if (source.Awaitable == null)
                return default;

            Awaitable<T>.Awaiter awaiter = source.Awaitable.GetAwaiter();
            return awaiter.GetResult();
        }
    }
}