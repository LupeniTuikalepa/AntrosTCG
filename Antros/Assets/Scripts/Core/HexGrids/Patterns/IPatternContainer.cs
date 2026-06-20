using ATCG.HexGrids.Patterns.Building;
using Helteix.Tools.DataMapping;

namespace ATCG.HexGrids.Patterns
{
    /// <summary>
    /// Non-generic handle the builder/mapper use. Bridges PatternData (base) to
    /// the concrete <see cref="IHexPattern{TData}"/>. <c>AddToBuilder</c> is
    /// generic over the controller so a struct controller flows through unboxed.
    /// </summary>
    public interface IPatternContainer : IContainer<PatternData>
    {
        void AddToBuilder<TController>(PatternData data, HexPatternBuilder<TController> builder)
            where TController : IHexPatternController;

        void AddToBuilder<TController>(PatternData data, HexPatternBuilder<TController> builder, HexCoordinates origin)
            where TController : IHexPatternController;
    }
}