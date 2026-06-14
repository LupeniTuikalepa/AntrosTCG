using System;
using System.Collections.Generic;
using System.Threading;
using Helteix.Tools.Phases.Listeners;
using UnityEngine;
using UnityEngine.Pool;

namespace Helteix.Tools.Phases
{
    /// <summary>
    /// All methods must be called from the Unity main thread.
    /// </summary>
    public static class PhaseManager
    {
        private static readonly ListenerComparer Comparer = new();

        // Listeners indexed by their target type for O(k) lookup instead of O(n).
        private static readonly Dictionary<Type, List<IPhaseListenerContainer>> ListenersByType = new();

        // Cache: maps a concrete phase type to its compatible listener lists.
        // Invalidated on every Register/Unregister.
        private static readonly Dictionary<Type, List<IPhaseListenerContainer>> ListenerCache = new();

        // No locking needed — main-thread only, matching Unity's Awaitable model.
        private static readonly Dictionary<object, CancellationTokenSource> RunningPhases = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize()
        {
            ListenersByType.Clear();
            ListenerCache.Clear();
            RunningPhases.Clear();
        }

        private static async Awaitable RunAsyncWithCallback<T, TResult>(this T phase, Action<TResult> onCompleted = null)
            where T : Phase<TResult>
        {
            try
            {
                TResult result = await Run(phase);
                await Awaitable.MainThreadAsync();
                onCompleted?.Invoke(result);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public static Awaitable<PhaseResult<TResult>>.Awaiter GetAwaiter<TResult>(this Phase<TResult> phase)
        {
            if (phase.IsRunning())
                throw new InvalidOperationException($"Phase {phase.GetType().Name} is already running. use WaitAsync if you just want to wait for the result.");

            return Run(phase).GetAwaiter();
        }

        public static async Awaitable<PhaseResult<TResult>> WaitAsync<TResult>(this Phase<TResult> phase, CancellationToken token = default)
        {
            if (!phase.IsRunning())
                throw new InvalidOperationException($"Phase {phase.GetType().Name} is not running.");

            Awaitable<PhaseResult<TResult>>.Awaiter awaiter = phase.CompletionSource.Awaitable.GetAwaiter();

            if (token == CancellationToken.None)
            {
                while (!awaiter.IsCompleted)
                    await Awaitable.NextFrameAsync(token);
            }
            else
            {
                while (!awaiter.IsCompleted)
                {
                    token.ThrowIfCancellationRequested();
                    await Awaitable.NextFrameAsync(token);
                }
            }

            return awaiter.GetResult();
        }

        public static void RunAndForget<TResult>(this Phase<TResult> phase) => _ = Run(phase);

        public static async Awaitable<PhaseResult<TResult>> Run<TResult>(this Phase<TResult> phase)
        {
            if (phase == null)
                return default;


            if(phase is ISinglePhase { AllowQueuing: false } phaseSingleton && PhaseSingletonManager.HasAnyRunning(phaseSingleton.Channel))
            {
                Debug.Log("[Phase Manager] Could not begin SinglePhase because one is already running in the same channel");
                return new PhaseResult<TResult>(default, PhaseResultType.Failure);
            }

            PhaseResult<TResult> result = default;
            using (CancellationTokenSource source = new CancellationTokenSource())
            {
                PhaseExecutionContext<TResult> context = new PhaseExecutionContext<TResult>()
                {
                    phase = phase,
                    source = source,
                };

                try
                {
                    await BeginPhase(context);
                    // ExecutePhase already handles cancellation/failure internally and
                    // returns a fully-formed PhaseResult — keep it as-is, do NOT let the
                    // implicit PhaseResult<T> -> T operator strip the type back to Success.
                    result = await phase.ExecutePhase(context.source.Token);
                }
                catch (OperationCanceledException phaseCanceledException)
                {
                    Debug.Log($"{phase.GetType().Name} phase was cancelled because of : {phaseCanceledException.Message}");
                    result = new PhaseResult<TResult>(default, PhaseResultType.Cancel);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    result = new PhaseResult<TResult>(default, PhaseResultType.Failure);
                }
                finally
                {
                    await EndPhase(context);
                }
            }

            return result;
        }

        internal static async Awaitable BeginPhase<TResult>(PhaseExecutionContext<TResult> context)
        {
            Phase<TResult> phase = context.phase;

            if (phase.IsRunning())
            {
                Debug.LogError("Cannot start the same phase twice.");
                return;
            }

            if (phase is ISinglePhase { AllowMultipleInstances: false } phaseSingleton)
            {
                await PhaseSingletonManager.WaitForSingleInstance(phaseSingleton, context.source.Token);
            }

            Debug.Log("[Phase Manager] Beginning phase " + phase.GetType().Name);

            RunningPhases.Add(phase, context.source);

            using (ListPool<IPhaseListenerContainer>.Get(out var listeners))
            {
                await phase.InitializePhase(context.source.Token);

                GetListenersFor(listeners, phase);
                foreach (var listener in listeners)
                    listener.OnPhaseBegin(phase);
            }
        }

        internal static async Awaitable EndPhase<TResult>(PhaseExecutionContext<TResult> context)
        {
            Phase<TResult> phase = context.phase;
            if (!phase.IsRunning())
            {
                Debug.LogError("Cannot end a phase that has not started.");
                return;
            }

            Debug.Log("[Phase Manager] Ending phase " + phase.GetType().Name);
            using (ListPool<IPhaseListenerContainer>.Get(out var listeners))
            {
                GetListenersFor(listeners, phase);
                foreach (var listener in listeners)
                    listener.OnPhaseEnd(phase);

                await phase.DisposePhase(context.source.Token);
            }

            RunningPhases.Remove(phase);
        }

        public static bool Register<T>(Action<T> beginCallback, Action<T> endCallback, int executionOrder = 0) where T : IPhase
        {
            AddListener(typeof(T), new CallbackPhaseListener<T>(beginCallback, endCallback, executionOrder));
            return true;
        }

        public static bool Register<T>(this IPhaseListener<T> listener, int executionOrder = 0) where T : IPhase
        {
            AddListener(typeof(T), new DefaultPhaseListenerContainer<T>(listener, executionOrder));
            return true;
        }

        public static bool Unregister<T>(Action<T> beginCallback, Action<T> endCallback) where T : IPhase
        {
            if (!ListenersByType.TryGetValue(typeof(T), out var list))
                return false;

            int count = list.RemoveAll(ctx =>
            {
                if (ctx is CallbackPhaseListener<T> container)
                    return container.beginCallback == beginCallback && container.endCallback == endCallback;
                return false;
            });

            if (count > 0) InvalidateCache(typeof(T));
            return count > 0;
        }

        public static bool Unregister<T>(this IPhaseListener<T> listener) where T : IPhase
        {
            if (!ListenersByType.TryGetValue(typeof(T), out var list))
                return false;

            int count = list.RemoveAll(ctx =>
            {
                if (ctx is DefaultPhaseListenerContainer<T> container)
                    return container.listener == listener;
                return false;
            });

            if (count > 0) InvalidateCache(typeof(T));
            return count > 0;
        }

        internal static void GetListenersFor<TResult>(List<IPhaseListenerContainer> compatibles, Phase<TResult> phase)
        {
            compatibles.Clear();
            Type phaseType = phase.GetType();

            if (ListenerCache.TryGetValue(phaseType, out var cached))
            {
                foreach (var c in cached)
                {
                    if(c.Accepts(phase))
                        compatibles.Add(c);
                }
                return;
            }

            // Build and cache the list for this concrete phase type.
            using (ListPool<IPhaseListenerContainer>.Get(out var temp))
            {
                foreach (var (listenerType, list) in ListenersByType)
                {
                    if (listenerType.IsAssignableFrom(phaseType))
                        temp.AddRange(list);
                }
                temp.Sort(Comparer);

                cached = new List<IPhaseListenerContainer>(temp);
            }

            ListenerCache[phaseType] = cached;
            //Redo with cache this time (the cache doesn't account for runtime filters on listerners)
            GetListenersFor(compatibles, phase);
        }

        private static void AddListener(Type type, IPhaseListenerContainer container)
        {
            if (!ListenersByType.TryGetValue(type, out var list))
            {
                list = new List<IPhaseListenerContainer>();
                ListenersByType[type] = list;
            }
            list.Add(container);
            list.Sort(Comparer);
            InvalidateCache(type);
        }

        private static void InvalidateCache(Type listenerType)
        {
            using (ListPool<Type>.Get(out var toRemove))
            {
                foreach (var (phaseType, _) in ListenerCache)
                {
                    if (listenerType.IsAssignableFrom(phaseType))
                        toRemove.Add(phaseType);
                }

                foreach (var type in toRemove)
                    ListenerCache.Remove(type);
            }
        }

        public static IEnumerable<T> GetAll<T>() where T : IPhase
        {
            foreach (var keyValuePair in RunningPhases)
            {
                if (keyValuePair.Key is T compatible)
                    yield return compatible;
            }
        }

        public static void Cancel<T>(this T phase) where T : IPhase
        {
            RunningPhases[phase].Cancel(true);
        }

        public static bool IsRunning<T>(this T phase) where T : IPhase
        {
            return RunningPhases.ContainsKey(phase);
        }
    }
}