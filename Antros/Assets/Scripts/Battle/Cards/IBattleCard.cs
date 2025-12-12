using ATCG.Battle.Grids;
using ATCG.Cards;
using ATCG.HexGrids;

namespace ATCG.Battle.Cards
{
    public interface IBattleCard : IGameCard, IHexMember
    {
        int PlayerID { get; }
        public BattleGrid Grid { get; }

        void Deploy(BattleGrid grid, HexCoordinates coordinates);
        void OnCardMoved(HexCoordinates from, HexCoordinates to);
        void Leave();
    }
}