using ATCG.Battle.Cards.Capacities.Behaviours.Effects;
using ATCG.Battle.Cards.Capacities.Behaviours.Mapping;
using ATCG.Battle.Cards.Capacities.Behaviours.Patterns;
using ATCG.Battle.Grids.Patterns;
using ATCG.Capacities.Data;
using ATCG.Capacities.Data.Effects;
using UnityEngine;

namespace ATCG.Battle.Cards.Capacities
{
    public static class BattleDataMapper
    {
        private static readonly CapacityEffectMapper EffectContainer = new CapacityEffectMapper();
        private static readonly PatternMapper PatternContainer = new PatternMapper();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            PatternContainer.Clear();
            EffectContainer.Clear();

            PatternContainer.Add<FloodFillPatternData, FloodFillPatternGenerator, FloodFillPattern>();
            PatternContainer.Add<OffsetsPatternData, OffsetPatternGenerator, OffsetsPattern>();
            PatternContainer.Add<PointsPatternData, PointsPatternGenerator, PointsPattern>();
            PatternContainer.Add<RayPatternData, RayPatternGenerator, RayPattern>();
            PatternContainer.Add<RingPatternData, RingPatternGenerator, RingPattern>();
            PatternContainer.Add<SpiralPatternData, SpiralPatternGenerator, SpiralPattern>();
            PatternContainer.Add<SpreadPatternData, SpreadPatternGenerator, SpreadPattern>();

            EffectContainer.Add<DamageEffectData, DamageEffect>();
            EffectContainer.Add<HealEffectData, HealEffect>();
        }
        static BattleDataMapper()
        {
        }

        public static bool TryGetFor(IEffectData data, out CapacityEffectMapper.IEffectContainer effect)
        {
            return EffectContainer.TryGetContainer(data, out effect);
        }

        public static bool TryGetFor(PatternData data, out PatternMapper.IPatternContainer pattern)
        {
            return PatternContainer.TryGetContainer(data, out pattern);
        }

    }
}