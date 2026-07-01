using ATCG.Battle.CapacitySystem.Core.Cutscenes;
using UnityEngine;
using UnityEngine.Playables;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Timeline
{
    /// <summary>
    /// Runtime behaviour for a QTE clip. Bridges the clip to the cutscene
    /// (IQteWindowHost) bound on the track. The host does the real work (gauge,
    /// input on owner screen, scoring); this just reports enter/tick/exit and the
    /// normalized position inside the clip.
    /// </summary>
    public class QtePlayableBehaviour : PlayableBehaviour
    {
        public QteClipData data;

        private IQteWindowHost host;
        private bool entered;


        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            host = playerData as IQteWindowHost;
            if (host == null)
                return;

            if (!entered)
            {
                entered = true;
                host.OnQteWindowEnter(data);
            }

            double duration = playable.GetDuration();
            double t = duration > 0d ? playable.GetTime() / duration : 0d;
            host.OnQteWindowTick(data, t);
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            // Fired when the clip ends (or the graph pauses on it). Treat the clip
            // end as the window close. Guard with `entered` so a pause before play
            // doesn't fire a spurious exit.
            if (entered && host != null)
            {
                entered = false;
                host.OnQteWindowExit(data);
            }
        }
    }
}