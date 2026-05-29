using System.Threading;
using UnityEngine;

namespace Helteix.Tools.Phases
{
    /// <summary>
    /// A phase that completes when <see cref="SetResult"/> or <see cref="Cancel"/> is called externally.
    /// Useful for phases driven by external events (e.g. waiting for a UI button press).
    /// <para>
    /// <b>Must be called from the Unity main thread.</b> This class is not thread-safe —
    /// do not call <see cref="SetResult"/> or <see cref="Cancel"/> from a background thread.
    /// If called from a background thread by mistake, the call will be marshalled to the main
    /// thread via <see cref="Awaitable.MainThreadAsync"/> before completing the phase.
    /// </para>
    /// </summary>
    public class PhaseCompletionSource<TValue> : Phase<TValue>
    {
        private readonly AwaitableCompletionSource<TValue> source;
        private bool _completed;

        protected PhaseCompletionSource()
        {
            source = new AwaitableCompletionSource<TValue>();
        }

        /// <summary>
        /// Sets the result and completes the phase. Has no effect if already completed or cancelled.
        /// </summary>
        public virtual void SetResult(in TValue value)
        {
            _ = SetResultAsync(value);
        }

        /// <summary>
        /// Cancels the phase. Has no effect if already completed.
        /// </summary>
        public void Cancel()
        {
            _ = CancelAsync();
        }

        private async Awaitable SetResultAsync(TValue value)
        {
            await Awaitable.MainThreadAsync();

            if (_completed)
                return;

            _completed = true;
            source.SetResult(value);
        }

        private async Awaitable CancelAsync()
        {
            await Awaitable.MainThreadAsync();

            if (_completed)
                return;

            _completed = true;
            source.SetCanceled();
        }

        protected override async Awaitable<TValue> Execute(CancellationToken token)
        {
            return await source.Awaitable;
        }

        protected override async Awaitable Initialize(CancellationToken token)
        {
            _completed = false;
            await Awaitable.MainThreadAsync();
        }

        protected override async Awaitable Dispose(CancellationToken token)
        {
            await Awaitable.MainThreadAsync();
        }
    }
}