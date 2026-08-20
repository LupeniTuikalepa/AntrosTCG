using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CutsceneEngine
{
    [TrackColor(0.2f, 0.75f, 1f)]
    [TrackBindingType(typeof(Animator))]
    [TrackClipType(typeof(HumanoidIKClip))]
    public class HumanoidIKTrack : TrackAsset
    {
        [Tooltip("Humanoid limb controlled by this track.")]
        public HumanoidIKTarget target = HumanoidIKTarget.LeftHand;

        [Tooltip("Automatically updates Left/Right and Hand/Foot tokens in this track's clip names when Target changes.")]
        public bool autoRenameClips = true;

        protected override void OnCreateClip(TimelineClip clip)
        {
            clip.duration = 1;
            clip.displayName = target.ToString();
            if (clip.asset is HumanoidIKClip humanoidClip)
            {
                humanoidClip.InitializeHumanoidSpaces();
                humanoidClip.InitializeGizmoColor(target);
            }
        }

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject gameObject, int inputCount)
        {
            var playable = ScriptPlayable<HumanoidIKMixerBehaviour>.Create(graph, inputCount);
            playable.GetBehaviour().target = target;
            return playable;
        }

        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            var animator = director.GetGenericBinding(this) as Animator;
            if (!HumanoidIKUtility.IsUsableHumanoid(animator)) return;

            if (HumanoidIKUtility.TryGetLimbBones(animator, target, out var limb))
            {
                AddTransformRotation(driver, limb.Upper);
                AddTransformRotation(driver, limb.Lower);
                AddTransformRotation(driver, limb.End);
            }

            if (HumanoidIKUtility.GetToeRigKind(animator, target) == HumanoidIKToeRigKind.ArticulatedToes)
            {
                AddTransformRotation(driver, HumanoidIKUtility.GetToeRoot(animator, target));
            }

            var digitChains = HumanoidIKDigitChainCache.GetChains(animator, target);
            foreach (var chain in digitChains)
            {
                foreach (var bone in chain)
                {
                    AddTransformRotation(driver, bone);
                }
            }
        }

        static void AddTransformRotation(IPropertyCollector driver, Transform transform)
        {
            if (!transform) return;

            driver.AddFromName<Transform>(transform.gameObject, "m_LocalRotation.x");
            driver.AddFromName<Transform>(transform.gameObject, "m_LocalRotation.y");
            driver.AddFromName<Transform>(transform.gameObject, "m_LocalRotation.z");
            driver.AddFromName<Transform>(transform.gameObject, "m_LocalRotation.w");
        }
    }
}
