using System;
using System.Collections.Generic;
using ATCG.Battle;                        // BattleID
using ATCG.Battle.Players.Local.Runtime;  // RuntimeLocalBattlePlayer
using ATCG.Metrics;                        // GameMetrics

namespace ATCG.Cutscenes
{
    /// <summary>
    /// Owns the in-flight QTE windows for ONE screen and arbitrates input across them: each open clip
    /// registers/updates its window, a single press resolves the most-progressed window (FIFO-ish, by
    /// normalized time), scoring 1 inside the critical window else 0, and a window that closes
    /// unresolved submits 0. This is the reusable QTE mechanic every cutscene host delegates to — it
    /// knows nothing about capacities, phases or networking.
    /// </summary>
    public sealed class QteWindowArbiter
    {
        private readonly Dictionary<BattleID, Qte> windows = new();
        private readonly RuntimeLocalBattlePlayer screenPlayer;
        private readonly IQteResultReceiver receiver;

        public event Action<Qte> WindowOpened;
        public event Action<Qte> WindowClosed;
        public event Action<Qte> Resolved;

        public QteWindowArbiter(RuntimeLocalBattlePlayer screenPlayer, IQteResultReceiver receiver)
        {
            this.screenPlayer = screenPlayer;
            this.receiver = receiver;
        }

        /// <summary>Called by a QTE clip each frame it's active; opens the window on first sight and
        /// closes it (submitting 0 if never resolved) once its time reaches its duration.</summary>
        public void SetQteData(BattleID qteID, QteClipData data, double time, double duration)
        {
            if (!windows.TryGetValue(qteID, out Qte target))
            {
                target = new Qte(screenPlayer, duration, data);
                windows.Add(qteID, target);
                WindowOpened?.Invoke(target);
            }

            target.SetDuration(duration);
            target.SetTime(time);

            if (time >= duration)
            {
                if (!target.IsDone)
                    receiver?.SubmitQteResult(0f);

                windows.Remove(qteID);
                WindowClosed?.Invoke(target);
            }
        }

        /// <summary>Attributes a single press to the most-progressed open window, scores and resolves
        /// it, and submits the score. No-op when no window is open.</summary>
        public void ResolvePress()
        {
            Qte target = null;
            double lastNorm = 0;

            foreach ((BattleID _, Qte qte) in windows)
            {
                if (qte.NormalizedTime > lastNorm)
                {
                    target = qte;
                    lastNorm = qte.NormalizedTime;
                }
            }

            if (target == null)
                return;

            float criticalPortion = GameMetrics.Current.QTESuccessRange;
            float threshold = 1f - criticalPortion;
            float score = target.NormalizedTime >= threshold ? 1f : 0f;

            target.Resolve();
            receiver?.SubmitQteResult(score);
            Resolved?.Invoke(target);
        }

        public void Clear() => windows.Clear();
    }
}
