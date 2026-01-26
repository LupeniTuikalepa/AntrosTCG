using ATCG.Battle.Cards;
using ATCG.Battle.Grids;
using ATCG.HexGrids;

namespace ATCG.Battle.Players.Local.Phases
{
    public interface IBattleCellLookupFilter
    {
        void Initialize(BattleGrid grid, IBattleCard card) { }
        void Dispose(BattleGrid grid, IBattleCard card) { }
        bool Accepts(BattleGrid grid, HexCoordinates coordinates, IBattleCard battleCard);
    }
}