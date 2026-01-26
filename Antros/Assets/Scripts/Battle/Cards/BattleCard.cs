using System;
using ATCG.Battle.Grids;
using ATCG.Cards;
using ATCG.HexGrids;
using ATCG.HexGrids.Grids;

namespace ATCG.Battle.Cards
{
    public abstract class BattleCard<T> : GameCard<T>, IBattleCard where T : GameCardData
    {
        public event Action<HexCoordinates, HexCoordinates> OnCardMoved;

        public bool IsDeployed => Grid != null;
        int IBattleCard.PlayerID => playerID;
        public HexCoordinates Coordinates { get; private set; }
        public BattleGrid Grid { get; private set; }

        public readonly int playerID;

        protected BattleCard(T data, int playerID) : base(data)
        {
            this.playerID = playerID;
        }

        void IBattleCard.Deploy(BattleGrid grid, HexCoordinates coordinates)
        {
            Coordinates = coordinates;
            Grid = grid;
            OnDeploy();
        }

        void IBattleCard.MoveCard(HexCoordinates from, HexCoordinates to)
        {
            Coordinates = to;
            MoveCard(from, to);

            OnCardMoved?.Invoke(from, to);
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
        protected virtual void MoveCard(HexCoordinates from, HexCoordinates to) { }
        protected virtual void EnterCell(BattleCell cell) { }
        protected virtual void LeaveCell(BattleCell cell) { }

    }
}