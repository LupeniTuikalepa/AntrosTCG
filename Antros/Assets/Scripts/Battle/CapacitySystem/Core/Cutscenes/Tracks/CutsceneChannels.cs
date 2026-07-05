// CutsceneChannels.cs

using ATCG.Battle.Entities.Runtime;
using ATCG.Battle.Entities.Runtime.Animations;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Timeline;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks
{
    public static class CutsceneChannels
    {
        public static readonly AutoBindChannel HeroAnimator =
            AutoBindChannel.Create<AnimationTrack>("HeroAnimator", ResolveHeroAnimator, ResolveDebugHeroAnimator);

        public static readonly AutoBindChannel MainCamera =
            AutoBindChannel.Create<CinemachineTrack>("MainCamera", ResolveCameraBrain, ResolveDebugCameraBrain);

        public static readonly AutoBindChannel[] All = { HeroAnimator, MainCamera };

        public static bool IsAutoBindableTrack(TrackAsset track)
        {
            for (int i = 0; i < All.Length; i++)
            {
                var autoBindChannel = All[i];

                if (track.name == autoBindChannel.trackName)
                {
                    if (track.GetType() == autoBindChannel.trackType)
                        return true;

                    Debug.LogWarning($"The track {track.name} is not of type {autoBindChannel.trackType} but {track.GetType()}");
                    return false;
                }
            }
            return false;
        }

        public static bool TryGetBinding(string channel, CutsceneBindContext ctx, out Object binding)
        {
            for (int i = 0; i < All.Length; i++)
            {
                var autoBindChannel = All[i];
                if (channel != autoBindChannel.trackName)
                    continue;

                binding = autoBindChannel.resolve(ctx);
                return binding != null;
            }

            binding = null;
            return false;
        }

        private static Object ResolveHeroAnimator(CutsceneBindContext ctx)
        {
            if (!ctx.phase.caster.IsValid)
                return null;
            if (!ctx.screenPlayer.RuntimeEntityManager.TryGetRuntimeEntity(ctx.phase.caster, out IRuntimeEntity entity))
                return null;
            return entity is IRuntimeEntityWithAnimator withAnimator ? withAnimator.Animator : null;
        }

        private static Object ResolveCameraBrain(CutsceneBindContext ctx)
        {
            return ctx.screenPlayer.Camera.Component.CinemachineBrain;
        }

        private static Object ResolveDebugHeroAnimator(DebugCutsceneRig rig) => rig.HeroAnimator;

        private static Object ResolveDebugCameraBrain(DebugCutsceneRig rig) => rig.CinemachineBrain;
    }
}