using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CutsceneEngine
{
    [TrackColor(0.62f, 0.38f, 0.95f)]
    [TrackBindingType(typeof(Animator))]
    [TrackClipType(typeof(LookAtClip))]
    public sealed class LookAtTrack : TrackAsset
    {
        protected override void OnCreateClip(TimelineClip clip)
        {
            clip.duration = 1d;
            clip.displayName = "Look At";
        }

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject gameObject, int inputCount)
        {
            var playable = ScriptPlayable<LookAtMixerBehaviour>.Create(graph, inputCount);
            playable.GetBehaviour().sourceTrack = this;
            return playable;
        }

        public override void GatherProperties(
            PlayableDirector director,
            IPropertyCollector driver)
        {
            var animator = director.GetGenericBinding(this) as Animator;
            if (!animator) return;

            LookAtUtility.GatherLookAtBoneRotations(
                animator,
                this,
                driver);
            LookAtUtility.GatherEyelidBlendShapes(
                animator,
                GetClips(),
                driver);
        }

    }
}
