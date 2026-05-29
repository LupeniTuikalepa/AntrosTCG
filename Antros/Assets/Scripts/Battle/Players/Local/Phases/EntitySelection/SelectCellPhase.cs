using System.Collections.Generic;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Grids;
using ATCG.HexGrids;
using Helteix.ChanneledProperties;
using Helteix.Tools.Phases;

namespace ATCG.Battle.Players.Local.Phases
{
    public class SelectCellPhase : SelectEntityPhase<SelectCellPhase.BattleCellFilter>
    {
        public struct BattleCellFilter : IEntityFilter
        {
            public bool Accepts(EntityAddress address) => address.IsBattleCellAspect();
        }

        private const string SELECT_CELL_PHASE_NAME = nameof(SelectCellPhase);

        public readonly BattleGrid battleGrid;

        protected readonly List<HexCoordinates> choices;

        public SelectCellPhase(List<HexCoordinates> choices, BattleGrid battleGrid, LocalBattlePlayer player) : base(new BattleCellFilter())
        {
            this.choices = choices;
            this.battleGrid = battleGrid;
            Player = player;

            MainChannelKey = ChannelKey.GetUniqueChannelKey($"{SELECT_CELL_PHASE_NAME}_Main");
            SecondaryChannelKey = ChannelKey.GetUniqueChannelKey($"{SELECT_CELL_PHASE_NAME}_Secondary");
        }

        public ChannelKey MainChannelKey { get; }
        public ChannelKey SecondaryChannelKey { get; }
        public LocalBattlePlayer Player { get; }

        public bool IsCoordValid(HexCoordinates coord)
        {
            return choices.Contains(coord);
        }


    }
}