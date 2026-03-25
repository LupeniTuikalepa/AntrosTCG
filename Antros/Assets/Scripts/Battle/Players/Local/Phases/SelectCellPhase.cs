using System.Collections.Generic;
using System.Threading;
using ATCG.Battle.Grids;
using ATCG.HexGrids;
using Helteix.ChanneledProperties;
using Helteix.Tools.Phases;
using UnityEngine;

namespace ATCG.Battle.Players.Local.Phases
{
    public class SelectCellPhase : PhaseCompletionSource<BattleCell>
    {
        private const string SELECT_CELL_PHASE_NAME = nameof(SelectCellPhase);

        public ChannelKey MainChannelKey { get; }
        public ChannelKey SecondaryChannelKey { get; }
        public LocalBattlePlayer Player { get;  }

        public readonly BattleGrid battleGrid;

        protected readonly List<HexCoordinates> choices;

        public SelectCellPhase(List<HexCoordinates> choices, BattleGrid battleGrid, LocalBattlePlayer player)
        {
            this.choices = choices;
            this.battleGrid = battleGrid;
            Player = player;

            MainChannelKey = ChannelKey.GetUniqueChannelKey($"{SELECT_CELL_PHASE_NAME}_Main");
            SecondaryChannelKey = ChannelKey.GetUniqueChannelKey($"{SELECT_CELL_PHASE_NAME}_Secondary");

        }
        public bool IsCoordValid(HexCoordinates coord) => choices.Contains(coord);

        public void SetResult(HexCoordinates coord)
        {
            if(IsCoordValid(coord))
                return;

            if(battleGrid.TryGetBattleCell(coord, out var cell))
                SetResult(cell);
        }
    }
}