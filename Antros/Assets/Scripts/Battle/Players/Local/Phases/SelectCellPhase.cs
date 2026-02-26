using System.Threading;
using ATCG.Battle.Grids;
using ATCG.Battle.Players.Local.Phases.Filters;
using ATCG.HexGrids;
using Helteix.ChanneledProperties;
using Helteix.Tools.Phases;
using UnityEngine;

namespace ATCG.Battle.Players.Local.Phases
{
    public class SelectCellPhase : PhaseCompletionSource<BattleCell>
    {
        public ChannelKey MainChannelKey { get; }
        public ChannelKey SecondaryChannelKey { get; }
        public ICellFilter Filter { get; }
        public LocalBattlePlayer Player { get;  }

        public readonly BattleGrid battleGrid;

        public SelectCellPhase(ICellFilter filter, BattleGrid battleGrid, LocalBattlePlayer player)
        {
            this.Filter = filter;
            this.battleGrid = battleGrid;
            Player = player;
            string selectCellPhaseName = nameof(SelectCellPhase);
            MainChannelKey = ChannelKey.GetUniqueChannelKey($"{selectCellPhaseName}_Main");
            SecondaryChannelKey = ChannelKey.GetUniqueChannelKey($"{selectCellPhaseName}_Secondary");

        }

        protected override Awaitable Initialize(CancellationToken token)
        {
            Filter?.Initialize(battleGrid);
            return base.Initialize(token);
        }

        public bool IsCoordValid(HexCoordinates coord) => Filter != null && Filter.Accepts(battleGrid, coord);

        protected override Awaitable Dispose(CancellationToken token)
        {
            Filter?.Dispose(battleGrid);
            return base.Dispose(token);
        }
    }
}