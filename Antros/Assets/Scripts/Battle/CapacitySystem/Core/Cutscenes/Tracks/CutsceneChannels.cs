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
            AutoBindChannel.Create<AnimationTrack>("HeroAnimator", ResolveHeroAnimator);

        public static readonly AutoBindChannel MainCamera =
            AutoBindChannel.Create<AnimationTrack>("MainCamera", ctx => ctx.screenPlayer.Camera.Component.CinemachineBrain);

        public static readonly AutoBindChannel[] All = { HeroAnimator, MainCamera };

        private static Object ResolveHeroAnimator(CutsceneBindContext ctx)
        {
            if (!ctx.phase.caster.IsValid)
                return null;
            if (!ctx.screenPlayer.RuntimeEntityManager.TryGetRuntimeEntity(ctx.phase.caster, out IRuntimeEntity entity))
                return null;
            return entity is IRuntimeEntityWithAnimator withAnimator ? withAnimator.Animator : null;
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
    }
}