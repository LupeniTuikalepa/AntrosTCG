using UnityEngine;
using UnityEngine.Playables;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Loops
{
    /// <summary>
    /// Runtime behaviour of a loop clip. Receives its host directly from the clip
    /// (resolved ExposedReference), so the track needs no binding. Reports bounds on
    /// enter and asks the host whether to loop again on exit.
    /// </summary>
    public class LoopClipBehaviour : PlayableBehaviour
    {
        public double clipStart;
        public double clipEnd;
        public ILoopClipHost host;

        private bool entered;

        // Reports the segment start once, giving the host the rewind bounds.
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (!Application.isPlaying || host == null)
                return;

            if (!entered)
            {
                entered = true;
                host.OnLoopSegmentStart(clipStart, clipEnd);
            }
        }

        // Asks the host to run the turn and decide whether to loop again.
        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (!Application.isPlaying || host == null)
                return;

            if (entered)
            {
                entered = false;
                host.OnLoopSegmentEnd();
            }
        }
    }
}