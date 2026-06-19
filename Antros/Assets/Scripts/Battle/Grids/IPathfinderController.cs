using ATCG.Battle.Entities.Aspects;
using ATCG.HexGrids;

namespace ATCG.Battle.Grids
{
    public interface IPathfinderController
    {
        bool CanTraverse(BattleCellAspect cell);
        int GetCost(HexCoordinates from, HexCoordinates to, BattleCellAspect cell);
        bool TryRedirect(HexCoordinates from, BattleCellAspect to, out HexCoordinates newCoordinates);
    }
}