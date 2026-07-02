using ATCG.Battle.Players.Local.Runtime;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.QTEs
{
    /// <summary>
    /// State of an in-flight QTE window. Created by the clip when it opens, it
    /// tracks its progress [0,1] and whether it has already been resolved. The
    /// cutscene uses it for FIFO arbitration and score computation. One window per
    /// clip means overlapping QTEs each keep their own independent state.
    /// </summary>
    public class Qte
    {
        public readonly RuntimeLocalBattlePlayer screenPlayer;

        public readonly QteClipData data;

        public double CurrentTime { get; private set; }
        public double Duration { get; private set; }
        public bool Resolved { get; private set; }

        public bool IsDone { get; private set; }
        public double NormalizedTime => CurrentTime / Duration;

        public Qte(RuntimeLocalBattlePlayer screenPlayer, double duration, QteClipData data)
        {
            this.screenPlayer = screenPlayer;
            this.data = data;
            Duration = duration;
        }

        public void SetTime(double time)
        {
            CurrentTime = time;
        }

        public void SetDuration(double duration)
        {
            Duration = duration;
        }

        public void Resolve()
        {
            if(IsDone)
                return;

            IsDone = true;
            Resolved = true;
        }
    }
}