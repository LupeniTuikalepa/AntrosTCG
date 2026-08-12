using ATCG.Battle.Entities.Runtime;
using ATCG.Battle.Entities.Runtime.Animations;
using ATCG.Cutscenes;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks
{
    /// <summary>
    /// Capacity-side resolution of the generic <see cref="CutsceneChannels"/> to live objects during a
    /// cast: the caster's Animator for the HeroAnimator channel, the screen camera's brain for
    /// MainCamera. This is the consumer half of the binding system — the channel identities are shared
    /// and generic, but "what does this channel bind to right now" depends on the capacity's cast
    /// context, so it lives here.
    /// </summary>
    public static class CapacityCutsceneBindings
    {
        public static bool TryGetBinding(string channel, CutsceneBindContext ctx, out Object binding)
        {
            if (channel == CutsceneChannels.HeroAnimator.trackName)
            {
                binding = ResolveHeroAnimator(ctx);
                return binding != null;
            }

            if (channel == CutsceneChannels.MainCamera.trackName)
            {
                binding = ResolveCameraBrain(ctx);
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
    }
}
