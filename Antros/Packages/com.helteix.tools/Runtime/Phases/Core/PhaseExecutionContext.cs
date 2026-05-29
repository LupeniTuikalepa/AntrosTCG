using System.Collections.Generic;
using System.Threading;

namespace Helteix.Tools.Phases
{
    internal struct PhaseExecutionContext<TResult>
    {
        public Phase<TResult> phase;
        public CancellationTokenSource source;
    }
}