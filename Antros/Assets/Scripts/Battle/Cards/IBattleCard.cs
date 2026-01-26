using ATCG.Battle.Grids;
using ATCG.Cards;
using ATCG.HexGrids;

namespace ATCG.Battle.Cards
{
    public interface IBattleCard : IGameCard, IHexMember
    {
        int PlayerID { get; }
        public BattleGrid Grid { get; }
        public bool IsDeployed { get; }

        public HexCoordinates Coordinates => IsDeployed && Grid != null ? Grid.GetCardCoordinates(this) : HexCoordinates.None;

        void Deploy(BattleGrid grid, HexCoordinates coordinates);
        void MoveCard(HexCoordinates from, HexCoordinates to);
        void Leave();
    }
}