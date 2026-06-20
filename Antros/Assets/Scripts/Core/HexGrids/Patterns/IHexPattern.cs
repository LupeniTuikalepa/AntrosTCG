using System.Collections.Generic;
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
        IEnumerable<HexCoordinates> GetAll<TController>(HexCoordinates from, TController controller)
            where TController : IHexPatternController;
    }

    [MappedBehaviour(typeof(PatternContainer<,>), typeof(IPatternContainer))]
    public interface IHexPattern<in TData> : IBehaviour<TData>
        where TData : PatternData
    {
        IEnumerable<HexCoordinates> GetAll<TController>(HexCoordinates from, TData data, TController controller)
            where TController : IHexPatternController;
    }
}