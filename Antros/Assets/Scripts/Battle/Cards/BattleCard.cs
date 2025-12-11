
using ATCG.Battle.HexGrids;
using ATCG.Cards;
using ATCG.HexGrids;
using ATCG.HexGrids.Grids;

namespace ATCG.Battle.Cards
{
    public abstract class BattleCard<T> : GameCard<T>, IBattleCard where T : GameCardData
    {
        public bool IsDeployed => Grid != null;
        int IBattleCard.PlayerID => playerID;

        public BattleGrid Grid { get; private set; }

        public readonly int playerID;

        protected BattleCard(T data, int playerID) : base(data)
        {
            this.playerID = playerID;
        }

        void IBattleCard.Deploy(BattleGrid grid, HexCoordinates coordinates)
        {
            Grid = grid;
            OnDeploy();
        }

        void IBattleCard.OnCardMoved(HexCoordinates from, HexCoordinates to)
        {
            OnCardMoved(from, to);
        }

        void IBattleCard.Leave()
        {
            Grid = null;
        }

        void IHexMember.EnterCell(HexCell hexCell)
        {
            if(Grid.TryGetBattleCell(hexCell, out BattleCell cell))
                EnterCell(cell);
        }

        void IHexMember.LeaveCell(HexCell hexCell)
        {
            if(Grid.TryGetBattleCell(hexCell, out BattleCell cell))
                LeaveCell(cell);
        }

        protected virtual void OnDeploy() { }
        protected virtual void OnCardMoved(HexCoordinates from, HexCoordinates to) { }
        protected virtual void EnterCell(BattleCell cell) { }
        protected virtual void LeaveCell(BattleCell cell) { }

    }
}