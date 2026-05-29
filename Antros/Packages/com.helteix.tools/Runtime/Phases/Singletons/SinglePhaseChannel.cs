using System.Collections.Generic;
using UnityEngine.Pool;

namespace Helteix.Tools.Phases
{
    internal class SinglePhaseChannel : IPhaseListener<ISinglePhase>
    {
        private readonly Queue<ISinglePhase> queue;
        private readonly HashSet<ISinglePhase> queued;
        public ISinglePhase CurrentPhase { get; private set; }

        public SinglePhaseChannel()
        {
            queue = new Queue<ISinglePhase>();
            queued = new HashSet<ISinglePhase>();
        }

        public bool EnqueueAndWait(ISinglePhase singlePhase)
        {
            queue.Enqueue(singlePhase);
            queued.Add(singlePhase);
            Refresh();

            return singlePhase == CurrentPhase;
        }

        public void Refresh()
        {
            if (CurrentPhase != null)
                return;

            if (queue.TryPeek(out ISinglePhase phase))
                CurrentPhase = phase;
            else
                CurrentPhase = null;
        }

        void IPhaseListener<ISinglePhase>.OnPhaseBegin(ISinglePhase phase)
        {
            if (queue.Count > 0)
            {
                queue.Dequeue();
                queued.Remove(phase);
            }
        }

        void IPhaseListener<ISinglePhase>.OnPhaseEnd(ISinglePhase phase)
        {
            if (phase == CurrentPhase)
                CurrentPhase = null;

            if (!queued.Contains(phase))
                return;

            // Should not happen
            using (ListPool<ISinglePhase>.Get(out var list))
            {
                list.AddRange(queue);
                list.Remove(phase);
                queued.Remove(phase);

                queue.Clear();
                foreach (var p in list)
                    queue.Enqueue(p);
            }
        }
    }
}