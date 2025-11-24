using ATCG.Battle.HexGrids;
using ATCG.Cards;
using ATCG.HexGrids;

namespace ATCG.Battle.Cards
{
    public interface IBattleCard : IGameCard, IHexMember
    {
        uint PlayerID { get; }
        public BattleGrid Grid { get; }

        void Deploy(BattleGrid grid, HexCoordinates coordinates);
        void OnCardMoved(HexCoordinates from, HexCoordinates to);
        void Leave();
    }
}