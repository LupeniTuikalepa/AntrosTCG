using ATCG.Battle.Cards.Capacities.Behaviours;
using ATCG.Battle.Cards.Capacities.Behaviours.Effects;
using ATCG.Battle.Cards.Capacities.Behaviours.Patterns;
using ATCG.Capacities.Data;
using ATCG.Capacities.Data.Effects;

namespace ATCG.Battle.Cards.Capacities
{
    public static class CapacityManager
    {
        private static readonly CapacityBehaviourContainer<ICapacityCastPattern, ICapacityCastPatternData>
            CastPatternContainer = new();

        private static readonly CapacityBehaviourContainer<ICapacityEffect, IEffectData> EffectContainer = new();

        static CapacityManager()
        {
            //Patterns
            CastPatternContainer.Add<FloodFillPatternData, FloodFillCapacityCastPattern>();
            CastPatternContainer.Add<SpreadPatternData, SpreadCapacityCastPattern>();

            //
            EffectContainer.Add<DamageEffectData, DamageEffect>();
            EffectContainer.Add<HealEffectData, HealEffect>();
        }

        public static bool TryGetFor(ICapacityCastPatternData data, out ICapacityCastPattern pattern)
        {
            return CastPatternContainer.TryGetFor(data, out pattern);
        }

        public static bool TryGetFor(IEffectData data, out ICapacityEffect effect)
        {
            return EffectContainer.TryGetFor(data, out effect);
        }
    }
}