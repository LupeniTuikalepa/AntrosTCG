using System;
using System.Threading;
using UnityEngine;

namespace Helteix.Tools.Phases
{
    public abstract class Phase<TResult> : IPhase
    {
        private const string CANCELLATION_REQUESTED_BY_THE_PHASE = "Cancellation requested by the phase";
        internal AwaitableCompletionSource<PhaseResult<TResult>> CompletionSource { get; } = new();

        public event Action OnInitialized;

        public event Action<TResult> OnCompleted;

        public event Action OnDisposed;

        public PhaseStatus CurrentStatus { get; private set; } = PhaseStatus.None;

        internal virtual Awaitable InitializePhase(CancellationToken token)
        {
            OnInitialized?.Invoke();
            CurrentStatus = PhaseStatus.None;
            CompletionSource.Reset();
            return Initialize(token);
        }

        internal virtual async Awaitable<PhaseResult<TResult>> ExecutePhase(CancellationToken token)
        {
            try
            {
                CurrentStatus = PhaseStatus.Running;

                // Await Execute directly. Unity's Awaitable propagates OperationCanceledException
                // through await, whereas manually polling the awaiter and calling GetResult()
                // does NOT reliably rethrow on cancellation — it can return default instead.
                TResult result = await Execute(token);

                // Catch a cancellation that completed Execute without surfacing as an exception.
                token.ThrowIfCancellationRequested();

                CurrentStatus = PhaseStatus.Completed;
                OnCompleted?.Invoke(result);

                var phaseResult = new PhaseResult<TResult>(result, PhaseResultType.Success);
                CompletionSource.TrySetResult(phaseResult);
                return phaseResult;
            }
            catch (OperationCanceledException)
            {
                CurrentStatus = PhaseStatus.Canceled;
                CompletionSource.TrySetCanceled();

                return new PhaseResult<TResult>(default, PhaseResultType.Cancel);
            }
            catch (Exception e)
            {
                CurrentStatus = PhaseStatus.Failed;
                CompletionSource.TrySetException(e);

                Debug.LogException(e);
                return new PhaseResult<TResult>(default, PhaseResultType.Failure);
            }
        }

        internal virtual Awaitable DisposePhase(CancellationToken token)
        {
            OnDisposed?.Invoke();

            CompletionSource?.Reset();
            return Dispose(token);
        }


        /// <summary>
        /// Execute the phase then send a value at the end
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        protected abstract Awaitable<TResult> Execute(CancellationToken token);

        /// <summary>
        /// Initialisation of phase. Called BEFORE every listener OnBegin.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        protected virtual async Awaitable Initialize(CancellationToken token)
        {
            await Awaitable.MainThreadAsync();
        }

        /// <summary>
        /// Completion of phase. Called AFTER every listener OnEnd.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        protected virtual async Awaitable Dispose(CancellationToken token)
        {
            await Awaitable.MainThreadAsync();
        }

        protected bool IsRunning() => PhaseManager.IsRunning(this);
        protected void Cancel() => PhaseManager.Cancel(this);

    }
    public abstract class Phase : Phase<Phase>
    {
        protected sealed override async Awaitable<Phase> Execute(CancellationToken token)
        {
            await ExecuteNoResult(token);
            return this;
        }

        protected abstract Awaitable ExecuteNoResult(CancellationToken token);
    }
}