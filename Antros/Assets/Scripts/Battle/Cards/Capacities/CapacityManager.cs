using ATCG.Battle.Cards.Capacities.Behaviours.Effects;
using ATCG.Battle.Cards.Capacities.Behaviours.Mapping;
using ATCG.Capacities.Data;
using ATCG.Capacities.Data.Effects;

namespace ATCG.Battle.Cards.Capacities
{
    public static class CapacityManager
    {
        private static readonly CapacityEffectMapper EffectContainer = new CapacityEffectMapper();
        private static readonly CapacityPatternMapper PatternContainer = new CapacityPatternMapper();

        static CapacityManager()
        {
            EffectContainer.Add<DamageEffectData, DamageEffect>();
            EffectContainer.Add<HealEffectData, HealEffect>();
        }

        public static bool TryGetFor(IEffectData data, out CapacityEffectMapper.IEffectContainer effect)
        {
            return EffectContainer.TryGetContainer(data, out effect);
        }

        public static bool TryGetFor(IHexCapacityPatternData data, out CapacityPatternMapper.IPatternContainer pattern)
        {
            return PatternContainer.TryGetContainer(data, out pattern);
        }

    }
}