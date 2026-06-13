using ATCG.Battle.Cards.Capacities.Behaviours.Effects;
using ATCG.Battle.Cards.Capacities.Behaviours.Mapping;
using ATCG.Battle.Cards.Capacities.Behaviours.Patterns;
using ATCG.Battle.Grids.Patterns;
using ATCG.Capacities.Data;
using ATCG.Capacities.Data.Effects;
using UnityEngine;

namespace ATCG.Battle.Cards.Capacities
{
    public static class CapacityManager
    {
        private static readonly CapacityEffectMapper EffectContainer = new CapacityEffectMapper();
        private static readonly CapacityPatternMapper PatternContainer = new CapacityPatternMapper();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            PatternContainer.Clear();
            EffectContainer.Clear();

            PatternContainer.Add<FloodFillPatternData, CapacityFloodFillPattern, FloodFillPattern>();
            PatternContainer.Add<OffsetsPatternData, CapacityOffsetPattern, OffsetsPattern>();
            PatternContainer.Add<PointsPatternData, CapacityPointsPattern, PointsPattern>();
            PatternContainer.Add<RayPatternData, CapacityRayPattern, RayPattern>();
            PatternContainer.Add<RingPatternData, CapacityRingPattern, RingPattern>();
            PatternContainer.Add<SpiralPatternData, CapacitySpiralPattern, SpiralPattern>();
            PatternContainer.Add<SpreadCapacityPatternData, CapacitySpreadPattern, SpreadPattern>();

            EffectContainer.Add<DamageEffectData, DamageEffect>();
            EffectContainer.Add<HealEffectData, HealEffect>();
        }
        static CapacityManager()
        {
        }

        public static bool TryGetFor(IEffectData data, out CapacityEffectMapper.IEffectContainer effect)
        {
            return EffectContainer.TryGetContainer(data, out effect);
        }

        public static bool TryGetFor(CapacityPatternData data, out CapacityPatternMapper.IPatternContainer pattern)
        {
            return PatternContainer.TryGetContainer(data, out pattern);
        }

    }
}