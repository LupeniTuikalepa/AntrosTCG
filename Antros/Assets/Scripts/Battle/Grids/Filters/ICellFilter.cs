using ATCG.Battle.Grids;
using ATCG.HexGrids;

namespace ATCG.Battle.Players.Local.Phases.Filters
{
    public interface ICellFilter
    {
        void Initialize(BattleGrid battleGrid);
        void Dispose(BattleGrid grid) { }
        bool Accepts(BattleGrid grid, HexCoordinates coordinates);
    }
}