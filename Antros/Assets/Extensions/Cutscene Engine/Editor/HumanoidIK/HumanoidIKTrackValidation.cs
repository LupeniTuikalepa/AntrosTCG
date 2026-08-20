using CutsceneEngine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CutsceneEngineEditor
{
    internal static class HumanoidIKTrackValidation
    {
        public static bool TryFindDuplicateTarget(
            HumanoidIKTrack track,
            Animator animator,
            PlayableDirector director,
            out HumanoidIKTrack duplicate)
        {
            duplicate = null;
            if (!track || !animator || !director || director.playableAsset is not TimelineAsset timeline)
            {
                return false;
            }

            foreach (var candidate in timeline.GetOutputTracks())
            {
                if (candidate == track || candidate is not HumanoidIKTrack humanoidTrack ||
                    humanoidTrack.target != track.target)
                {
                    continue;
                }

                if (director.GetGenericBinding(humanoidTrack) != animator) continue;

                duplicate = humanoidTrack;
                return true;
            }

            return false;
        }
    }
}
