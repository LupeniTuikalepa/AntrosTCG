using ATCG.Battle;
using UnityEngine;
using ATCG.Cutscenes;
using UnityEngine.Playables;

namespace ATCG.Cutscenes
{
    /// <summary>
    /// Behaviour for a QTE clip. Each clip owns ITS window (local state), which
    /// handles overlapping QTEs natively. It registers with the cutscene on open,
    /// updates its normalized time, and removes itself on close. It never touches
    /// input or emission — the cutscene arbitrates presses, the director emits.
    /// </summary>
    public class QtePlayableBehaviour : PlayableBehaviour
    {
        public QteClipData data;

        private IQteWindowHost host;
        private BattleID qteID;

        public override void OnPlayableCreate(Playable playable)
        {
            qteID = BattleID.CreateNew();
            base.OnPlayableCreate(playable);
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (!Application.isPlaying)
                return;

            host ??= ResolveHost(playable);
            if (host == null)
                return;

            double duration = playable.GetDuration();
            host.SetQteData(qteID, data, duration, duration);
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            // Do nothing in edit mode (scrub/preview): no play session, no host.
            // Prevents the editor from crashing when scrubbing a QTE timeline.
            if (!Application.isPlaying)
                return;

            host ??= ResolveHost(playable);
            if (host == null)
                return;

            double duration = playable.GetDuration();
            double time = playable.GetTime();

            host.SetQteData(qteID, data, time, duration);
        }

        // The QTE host is whatever cutscene component sits on the director's GameObject (capacity or
        // generic) — resolved from the graph rather than a track binding, so the QTE clip works on any
        // cutscene and the track needs no CapacityCutscene binding.
        private static IQteWindowHost ResolveHost(Playable playable)
        {
            PlayableDirector director = playable.GetGraph().GetResolver() as PlayableDirector;
            return director != null ? director.GetComponent<IQteWindowHost>() : null;
        }
    }
}
