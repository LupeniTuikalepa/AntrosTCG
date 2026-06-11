using System;
using System.Threading;
using UnityEngine;

namespace Helteix.Tools.Phases
{
    /// <summary>
    /// A phase that completes when <see cref="SetResult"/> or <see cref="SetCanceled"/> is called externally.
    /// Useful for phases driven by external events (e.g. waiting for a UI button press).
    /// <para>
    /// <b>Must be called from the Unity main thread.</b> This class is not thread-safe —
    /// do not call <see cref="SetResult"/> or <see cref="SetCanceled"/> from a background thread.
    /// If called from a background thread by mistake, the call will be marshalled to the main
    /// thread via <see cref="Awaitable.MainThreadAsync"/> before completing the phase.
    /// </para>
    /// </summary>
    public abstract class PhaseCompletionSource<TValue> : Phase<TValue>
    {
        // Separate from Phase<TResult>.CompletionSource — this one is driven by SetResult/SetCanceled.
        // Phase<TResult>.CompletionSource is driven by ExecutePhase and handles WaitAsync notifications.
        private readonly AwaitableCompletionSource<PhaseResult<TValue>> internalSource = new();

        protected PhaseCompletionSource() { }

        /// <summary>
        /// Sets the result and completes the phase. Has no effect if already completed or cancelled.
        /// </summary>
        public virtual void SetResult(in TValue value)
        {
            _ = SetResultAsync(value);
        }

        /// <summary>
        /// Cancels the phase immediately. Has no effect if already completed.
        /// </summary>
        protected void SetCanceled()
        {
            internalSource.TrySetResult(new PhaseResult<TValue>(default, PhaseResultType.Cancel));
        }

        private async Awaitable SetResultAsync(TValue value)
        {
            await Awaitable.MainThreadAsync();
            internalSource.TrySetResult(new PhaseResult<TValue>(value, PhaseResultType.Success));
        }

        protected override async Awaitable<TValue> Execute(CancellationToken token)
        {
            var awaiter = internalSource.Awaitable.GetAwaiter();
            while (!awaiter.IsCompleted)
            {
                token.ThrowIfCancellationRequested();
                await Awaitable.NextFrameAsync(token);
            }

            PhaseResult<TValue> result = awaiter.GetResult();
            if (result.type == PhaseResultType.Cancel)
                throw new OperationCanceledException(token);

            if (result.type == PhaseResultType.Failure)
                throw new Exception($"PhaseCompletionSource<{typeof(TValue).Name}> failed.");

            return result.value;
        }

        protected override async Awaitable Initialize(CancellationToken token)
        {
            internalSource.Reset();
            await Awaitable.MainThreadAsync();
        }

        protected override async Awaitable Dispose(CancellationToken token)
        {
            await Awaitable.MainThreadAsync();
        }
    }
}