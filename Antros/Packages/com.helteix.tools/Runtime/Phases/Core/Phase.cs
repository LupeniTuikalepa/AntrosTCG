using System;
using System.Threading;
using UnityEngine;

namespace Helteix.Tools.Phases
{
    public abstract class Phase<TResult> : IPhase
    {
        public event Action OnInitialized;

        public event Action<TResult> OnCompleted;

        public event Action OnDisposed;

        public PhaseStatus CurrentStatus { get; private set; } = PhaseStatus.None;


        internal virtual Awaitable InitializePhase(CancellationToken token)
        {
            OnInitialized?.Invoke();
            CurrentStatus = PhaseStatus.None;
            return Initialize(token);
        }

        internal virtual async Awaitable<PhaseResult<TResult>> ExecutePhase(CancellationToken token)
        {
            try
            {
                CurrentStatus = PhaseStatus.Running;
                var awaitable = Execute(token);
                Awaitable<TResult>.Awaiter awaiter = awaitable.GetAwaiter();
                while (!awaiter.IsCompleted)
                {
                    token.ThrowIfCancellationRequested();
                    await Awaitable.NextFrameAsync(token);
                }

                CurrentStatus = PhaseStatus.Completed;
                TResult result = awaiter.GetResult();
                OnCompleted?.Invoke(result);

                return new PhaseResult<TResult>(result, PhaseResultType.Success);
            }
            catch (OperationCanceledException)
            {
                CurrentStatus = PhaseStatus.Canceled;
                return new PhaseResult<TResult>(default, PhaseResultType.Cancel);
            }
            catch (Exception e)
            {
                CurrentStatus = PhaseStatus.Failed;
                Debug.LogException(e);
                return new PhaseResult<TResult>(default, PhaseResultType.Failure);
            }
        }

        internal virtual Awaitable DisposePhase(CancellationToken token)
        {
            OnDisposed?.Invoke();
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