using System.Collections.Generic;
using ATCG.HexGrids.Patterns.Building;
using Helteix.Tools.DataMapping;

namespace ATCG.HexGrids.Patterns
{
    /// <summary>
    /// A pattern is stateless: it reads its <typeparamref name="TData"/> at
    /// evaluation time and yields coordinates. <c>GetAll</c> is generic over the
    /// controller so a struct controller (e.g. BattlePatternController) is passed
    /// without boxing.
    /// </summary>
    public interface IHexPattern
    {
        IEnumerable<HexCoordinates> GetAll(HexCoordinates from, IHexPatternController controller);
    }

    [GenerateContainer]
    public interface IHexPattern<in TData> : IBehaviour<TData> where TData : PatternData
    {
        IEnumerable<HexCoordinates> GetAll(TData data, HexCoordinates from, IHexPatternController controller);

        [AddToContainer]
        void AddToBuilder(TData data, HexPatternBuilder builder, HexCoordinates origin)
        {
            if (data.OverridePatternOrigin) origin += data.Offset;
            foreach (var coord in GetAll(data, origin, builder.controller))
            {
                if (data.IsAdditive)
                    builder.With(coord);
                else
                    builder.Without(coord); }
        }
    }
}