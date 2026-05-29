using UnityEngine;

namespace Helteix.Tools.Phases.Utilities
{
    internal class WaitForPhaseResult<TValue> : WaitForPhase<Phase<TValue>>
    {
        public TValue CurrentResult { get; private set; }

        public WaitForPhaseResult(Phase<TValue> current) : base(current)
        {

        }


        public async Awaitable<TValue> WaitForValue()
        {
            await Wait();
            return CurrentResult;
        }

        internal override async Awaitable Wait()
        {
            current.OnCompleted += OnCompleted;

            await base.Wait();

            current.OnCompleted -= OnCompleted;
        }

        private void OnCompleted(TValue value) => CurrentResult = value;

    }
}