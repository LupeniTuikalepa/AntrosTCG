using System.Collections.Generic;

namespace ATCG.Battle.Grids
{
    public class PriorityQueue<TElement, TPriority>
    {
        private readonly List<(TElement element, TPriority priority)> elements = new();

        public int Count => elements.Count;

        public void Enqueue(TElement item, TPriority priority)
        {
            elements.Add((item, priority));
        }

        public TElement Dequeue()
        {
            Comparer<TPriority> comparer = Comparer<TPriority>.Default;
            int bestIndex = 0;

            for (int i = 1; i < elements.Count; i++)
            {
                if (comparer.Compare(elements[i].priority, elements[bestIndex].priority) < 0)
                    bestIndex = i;
            }

            TElement best = elements[bestIndex].element;
            elements.RemoveAt(bestIndex);
            return best;
        }

        public void Clear()
        {
            elements.Clear();
        }
    }
}