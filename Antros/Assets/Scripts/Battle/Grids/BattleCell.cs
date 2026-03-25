using System;
using System.Collections.Generic;
using System.Linq;
using ATCG.Battle.Cards;
using ATCG.Battle.Cards.Capacities;
using ATCG.Battle.Grids.Entities;
using ATCG.Battle.Grids.Entities.Heroes;
using ATCG.Battle.Players;
using ATCG.HexGrids;

namespace ATCG.Battle.Grids
{
    public class BattleCell : ICapacityTarget
    {
        public event Action<HeroBattleCard> Attacked;
        public HexCoordinates Coordinates => cell.coordinates;

        public readonly BattleGrid battleGrid;
        public readonly HexCell cell;

        public IEnumerable<GridEntity> Members => members;

        private HashSet<GridEntity> members = new();

        public BattleCell(BattleGrid battleGrid, HexCell cell)
        {
            this.battleGrid = battleGrid;
            this.cell = cell;
        }

        public bool CanHeroMoveTo() => Members.All(ctx => ctx is not HeroEntity);

        public bool CanBeDeployedOn(IBattlePlayer player) => Members.All(ctx => ctx is not HeroEntity);


        public void OnBasicAttackPerformed(HeroBattleCard card)
        {
            Attacked?.Invoke(card);
        }

        public bool CanBeAttacked(IBattleCard card)
        {
            /*
            foreach (var member in Members)
            {
                if (member is ICellDefence blocker && blocker.CanDefendAgainst(card))
                    return false;
            }*/

            return true;
        }

        public bool HasMember(GridEntity member) => members.Contains(member);

        public void AddMember(GridEntity member)
        {
            if (member != null)
            {
                members.Add(member);
                member.OnEnterCell(this);
            }
        }

        public void RemoveMember(GridEntity member)
        {
            if (member != null)
            {
                members.Remove(member);
                member.OnExitCell(this);
            }
        }
    }
}