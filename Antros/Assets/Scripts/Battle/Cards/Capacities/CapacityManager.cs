using ATCG.Battle.Cards.Capacities.Behaviours;
using ATCG.Battle.Cards.Capacities.Behaviours.Effects;
using ATCG.Battle.Grids.Patterns;
using ATCG.Capacities.Data;
using ATCG.Capacities.Data.Effects;

namespace ATCG.Battle.Cards.Capacities
{
    public static class CapacityManager
    {
        private static readonly CapacityDataMapper<ICellPattern, ICapacityPatternData> CastPatternContainer = new();

        private static readonly CapacityDataMapper<ICapacityEffect, IEffectData> EffectContainer = new();

        static CapacityManager()
        {
            //Patterns
            CastPatternContainer.Add<FloodFillPatternData, FloodFillPattern>();
            CastPatternContainer.Add<SpreadPatternData, SpreadPattern>();

            //
            EffectContainer.Add<DamageEffectData, DamageEffect>();
            EffectContainer.Add<HealEffectData, HealEffect>();
        }

        public static bool TryGetFor(ICapacityPatternData data, out ICellPattern pattern)
        {
            return CastPatternContainer.TryGetFor(data, out pattern);
        }

        public static bool TryGetFor(IEffectData data, out ICapacityEffect effect)
        {
            return EffectContainer.TryGetFor(data, out effect);
        }
    }
}