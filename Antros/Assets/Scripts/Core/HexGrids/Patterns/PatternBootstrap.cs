using Helteix.Tools.DataMapping;
using UnityEngine;

namespace ATCG.HexGrids.Patterns
{
    /// <summary>
    /// Owns the single PatternMapper and registers every data-driven pattern.
    /// Replaces the pattern half of BattleDataMapper. Dispatch via TryGetFor.
    /// </summary>
    public static class PatternBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            DomainBucket<IPatternContainer>.Clear();

            Mapper.Register<FloodFillPatternData, FloodFillPatternWithData>();
            Mapper.Register<PointsPatternData, PointsPatternWithData>();
            Mapper.Register<RayPatternData, RayPatternWithData>();
            Mapper.Register<RingPatternData, RingPatternWithData>();
            Mapper.Register<SpiralPatternData, SpiralPatternWithData>();
            Mapper.Register<SpreadPatternData, SpreadPatternWithData>();
            Mapper.Register<LinePatternData, LinePatternWithData>();
            Mapper.Register<EverythingPatternData, EverythingPatternWithData>();
        }
    }
}