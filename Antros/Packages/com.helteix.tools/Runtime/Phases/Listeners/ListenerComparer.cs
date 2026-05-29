using System.Collections.Generic;

namespace Helteix.Tools.Phases.Listeners
{
    internal class ListenerComparer : IComparer<IPhaseListenerContainer>
    {
        public int Compare(IPhaseListenerContainer x, IPhaseListenerContainer y)
        {
            if (ReferenceEquals(x, y)) return 0;

            if (y is null) return 1;
            if (x is null) return -1;

            return x.ExecutionOrder.CompareTo(y.ExecutionOrder);
        }
    }
}