using System.Threading;
using ATCG.Battle.Cards;
using ATCG.Battle.Grids;
using ATCG.HexGrids;
using Helteix.ChanneledProperties;
using Helteix.Tools.Phases;
using UnityEngine;

namespace ATCG.Battle.Players.Local.Phases
{
    public class BattleCardCellLookupPhase : PhaseCompletionSource<BattleCell>, IBattleCellLookupPhase
    {

        public int PlayerID => card.PlayerID;
        public ChannelKey ChannelKey { get; }
        public IBattleCellLookupFilter Filter { get; }


        public readonly BattleGrid battleGrid;
        public readonly IBattleCard card;


        public BattleCardCellLookupPhase(IBattleCellLookupFilter filter, BattleGrid battleGrid, IBattleCard card)
        {
            this.Filter = filter;
            this.battleGrid = battleGrid;
            this.card = card;
            ChannelKey = ChannelKey.GetUniqueChannelKey(nameof(BattleCardCellLookupPhase));
        }

        protected override Awaitable Initialize(CancellationToken token)
        {
            Filter?.Initialize(battleGrid, card);
            return base.Initialize(token);
        }

        public bool IsCoordValid(HexCoordinates coord) => Filter != null && Filter.Accepts(battleGrid, coord, card);

        protected override Awaitable Dispose(CancellationToken token)
        {
            Filter?.Dispose(battleGrid, card);
            return base.Dispose(token);
        }
    }
}