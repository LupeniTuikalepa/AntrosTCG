using System.Collections.Generic;

namespace ATCG.Cutscenes
{
    /// <summary>
    /// Accumulates the QTE scores that resolve between two steps and averages them when a step reads
    /// them. Reading marks the batch consumed, so the next added score starts a fresh batch — matching
    /// the runtime model where each step flushes exactly the QTEs that resolved since the previous
    /// step. Empty batch reads as full effectiveness (1). Reusable by any cutscene consumer.
    /// </summary>
    public sealed class QteResultAccumulator
    {
        private readonly List<float> results = new();
        private bool consumedSinceLastFill;

        public void Add(float score)
        {
            if (consumedSinceLastFill)
            {
                results.Clear();
                consumedSinceLastFill = false;
            }

            results.Add(score);
        }

        public float Read()
        {
            consumedSinceLastFill = true;

            if (results.Count == 0)
                return 1f;

            float sum = 0f;
            for (int i = 0; i < results.Count; i++)
                sum += results[i];
            return sum / results.Count;
        }

        public void Clear()
        {
            results.Clear();
            consumedSinceLastFill = false;
        }
    }
}
