using ATCG.HexGrids.Patterns.Building;
using Helteix.Tools.DataMapping;

namespace ATCG.HexGrids.Patterns
{
    /// <summary>
    /// Concrete container. The pattern struct lives in the base concrete field
    /// (never boxed). AddToBuilder matches the data's concrete type, then drives
    /// the builder's With/Without, applying the additive / override-origin rules.
    /// </summary>
    public sealed class PatternContainer<TData, TPattern> : Container<TData, TPattern>, IPatternContainer
        where TData : PatternData
        where TPattern : struct, IHexPattern<TData>
    {
        public PatternContainer(TPattern pattern) : base(pattern)
        {
        }

        public void AddToBuilder<TController>(PatternData data, HexPatternBuilder<TController> builder)
            where TController : IHexPatternController
            => AddToBuilder(data, builder, builder.origin);

        public void AddToBuilder<TController>(PatternData data, HexPatternBuilder<TController> builder, HexCoordinates origin)
            where TController : IHexPatternController
        {
            if (data is not TData typed)
                return;

            HexCoordinates source = data.OverridePatternOrigin ? origin + data.Offset : origin;

            foreach (var coord in behaviour.GetAll(source, typed, builder.controller))
            {
                if (data.IsAdditive)
                    builder.With(coord);
                else
                    builder.Without(coord);
            }
        }
    }
}