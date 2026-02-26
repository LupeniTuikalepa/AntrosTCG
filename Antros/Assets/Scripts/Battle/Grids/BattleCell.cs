using System;
using System.Linq;
using ATCG.Battle.Cards;
using ATCG.HexGrids;

namespace ATCG.Battle.Grids
{
    public class BattleCell
    {
        public event Action<HeroBattleCard> Attacked;

        public readonly BattleGrid battleGrid;
        public readonly HexCell cell;

        public BattleCell(BattleGrid battleGrid, HexCell cell)
        {
            this.battleGrid = battleGrid;
            this.cell = cell;
        }


        public bool CanBeDeployedOn() => cell.Members.All(ctx => ctx is not HeroBattleCard);

        public void OnBasicAttackPerformed(HeroBattleCard card)
        {
            Attacked?.Invoke(card);
        }

    }
}