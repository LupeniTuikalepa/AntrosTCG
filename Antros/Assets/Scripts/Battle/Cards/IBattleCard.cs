using ATCG.Battle.Cards.Capacities;
using ATCG.Battle.Grids;
using ATCG.Battle.Players;
using ATCG.Cards;
using ATCG.HexGrids;

namespace ATCG.Battle.Cards
{
    public interface IBattleCard : IGameCard, IHexMember
    {
        IBattlePlayer Player { get; }
        public BattleGrid BattleGrid { get; }

        public bool IsDeployed { get; }
        public HexCoordinates Coordinates { get; }

        void Deploy(BattleGrid grid, HexCoordinates coordinates);
        void Leave();

        void RegisterEventRunner<TEventRunner>(TEventRunner runner) where TEventRunner : ICardEventRunner;
        void UnregisterEventRunner<TEventRunner>(TEventRunner runner) where TEventRunner : ICardEventRunner;
    }
}