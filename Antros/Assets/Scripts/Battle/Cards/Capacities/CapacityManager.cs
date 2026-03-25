using ATCG.Battle.Cards.Capacities.Effects;
using ATCG.Capacities.Data;

namespace ATCG.Battle.Cards.Capacities.Patterns
{
    public static class CapacityManager
    {

        private static readonly CapacityBehaviourContainer<ICapacityCastPattern, ICapacityCastPatternData> CastPatternContainer = new();
        private static readonly CapacityBehaviourContainer<ICapacityHitEffect, ICapacityHitEffectData> HitEffectContainer = new();



        static CapacityManager()
        {
            //Patterns
            CastPatternContainer.Add<FloodFillPatternData, FloodFillCapacityCastPattern>();
            CastPatternContainer.Add<SpreadPatternData, SpreadCapacityCastPattern>();

            //

        }

        public static bool TryGetFor(ICapacityCastPatternData data, out ICapacityCastPattern pattern)
        {
            return CastPatternContainer.TryGetFor(data, out pattern);
        }
        public static bool TryGetFor(ICapacityHitEffectData data, out ICapacityHitEffect effect)
        {
            return HitEffectContainer.TryGetFor(data, out effect);
        }
    }
}